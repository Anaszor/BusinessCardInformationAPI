using BusinessCardInformationAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessCardInformationAPI.Application.Interfaces
{
    public interface IBusinessCardService
    {
        Task<IEnumerable<BusinessCardDto>> GetAllAsync(string? search = null);
        Task<BusinessCardDto?> GetByIdAsync(int id);
        Task AddAsync(CreateBusinessCardDto dto);
        Task DeleteAsync(int id);
        Task<byte[]> ExportToCsvAsync();
        Task<byte[]> ExportToXmlAsync();
    }
}