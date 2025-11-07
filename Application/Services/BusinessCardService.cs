using AutoMapper;
using BusinessCardInformationAPI.Application.DTOs;
using BusinessCardInformationAPI.Application.Interfaces;
using BusinessCardInformationAPI.Domain.Entities;
using BusinessCardInformationAPI.Domain.Interfaces;
using System.Text;
using System.Xml.Serialization;

namespace BusinessCardInformationAPI.Application.Services
{
    public class BusinessCardService : IBusinessCardService
    {
        private readonly IBusinessCardRepository _repository;
        private readonly IMapper _mapper;
        private const int MaxPhotoSizeBytes = 1 * 1024 * 1024; // 1 MB

        public BusinessCardService(IBusinessCardRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BusinessCardDto>> GetAllAsync(string? search = null) 
        {
            var entities = await _repository.GetAllAsync(search);
            return _mapper.Map<IEnumerable<BusinessCardDto>>(entities);
        }

        public async Task<BusinessCardDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<BusinessCardDto>(entity);
        }

        public async Task AddAsync(CreateBusinessCardDto dto)
        {
            // Validate base64 photo if provided
            if (!string.IsNullOrEmpty(dto.Photo))
            {
                if (!IsBase64(dto.Photo))
                    throw new ArgumentException("Photo is not a valid base64 string.");

                var photoBytes = Convert.FromBase64String(dto.Photo.Contains(",") ? dto.Photo.Split(',')[1] : dto.Photo);
                if (photoBytes.Length > MaxPhotoSizeBytes)
                    throw new ArgumentException("Photo exceeds maximum allowed size of 1 MB.");
            }

            var entity = _mapper.Map<BusinessCard>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
        }

        public async Task<byte[]> ExportToCsvAsync()
        {
            var cards = await _repository.GetAllForExportAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Gender,DateOfBirth,Email,Phone,Photo,Address");

            foreach (var c in cards)
            {
                sb.AppendLine($"{c.Id},{EscapeCsv(c.Name)},{c.Gender},{c.DateOfBirth:yyyy-MM-dd},{c.Email},{c.Phone},{c.Photo},{EscapeCsv(c.Address)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportToXmlAsync()
        {
            var cards = await _repository.GetAllForExportAsync();
            var serializer = new XmlSerializer(typeof(List<BusinessCard>));
            using var ms = new MemoryStream();
            serializer.Serialize(ms, cards.ToList());
            return ms.ToArray();
        }

        // Helper: Validate base64 string
        private bool IsBase64(string base64)
        {
            try
            {
                var cleanBase64 = base64.Contains(",") ? base64.Split(',')[1] : base64;
                Span<byte> buffer = new Span<byte>(new byte[cleanBase64.Length]);
                return Convert.TryFromBase64String(cleanBase64, buffer, out _);
            }
            catch
            {
                return false;
            }
        }

        // Helper: Escape CSV commas and quotes
        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\""))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
