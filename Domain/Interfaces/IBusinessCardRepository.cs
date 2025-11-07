using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BusinessCardInformationAPI.Domain.Entities;

namespace BusinessCardInformationAPI.Domain.Interfaces
{
    public interface IBusinessCardRepository
    {
        Task<IEnumerable<BusinessCard>> GetAllAsync(string? search = null);
        Task<BusinessCard?> GetByIdAsync(int id);
        Task AddAsync(BusinessCard businessCard);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<IEnumerable<BusinessCard>> GetAllForExportAsync();
    }
}
