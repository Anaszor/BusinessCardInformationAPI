using BusinessCardInformationAPI.Domain.Entities;
using BusinessCardInformationAPI.Domain.Interfaces;
using BusinessCardInformationAPI.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessCardInformationAPI.Infrastructure.Persistence.Repositories
{
    public class BusinessCardRepository : IBusinessCardRepository
    {
        private readonly ApplicationDbContext _context;

        public BusinessCardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusinessCard>> GetAllAsync(string? search = null)
        {
            var query = _context.BusinessCards.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(search) ||
                    b.Email.ToLower().Contains(search) ||
                    b.Phone.ToLower().Contains(search) ||
                    b.Address.ToLower().Contains(search));
            }

            return await query.ToListAsync();
        }


        public async Task<BusinessCard?> GetByIdAsync(int id)
        {
            return await _context.BusinessCards.FindAsync(id);
        }

        public async Task AddAsync(BusinessCard businessCard)
        {
            await _context.BusinessCards.AddAsync(businessCard);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.BusinessCards.FindAsync(id);
            if (entity != null)
                _context.BusinessCards.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BusinessCard>> GetAllForExportAsync()
        {
            return await _context.BusinessCards.AsNoTracking().ToListAsync();
        }
    }
}
