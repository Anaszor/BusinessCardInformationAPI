using Microsoft.EntityFrameworkCore;
using BusinessCardInformationAPI.Infrastructure.Persistence.Configurations;
using BusinessCardInformationAPI.Domain.Entities;

namespace BusinessCardInformationAPI.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<BusinessCard> BusinessCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BusinessCardConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
