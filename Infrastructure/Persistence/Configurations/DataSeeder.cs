using BusinessCardInformationAPI.Application.DTOs;
using BusinessCardInformationAPI.Application.Interfaces;
using BusinessCardInformationAPI.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IBusinessCardService _service;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(ApplicationDbContext context, IBusinessCardService service, ILogger<DataSeeder> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created and latest migrations applied
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database migration failed during seeding.");
                throw;
            }

            // If there are any existing records, skip seeding
            var existing = await _service.GetAllAsync(null);
            if (existing != null && existing.Any())
            {
                _logger.LogInformation("Database already contains business cards - seeding skipped.");
                return;
            }

            var samples = new[]
            {
                new CreateBusinessCardDto
                {
                    Name = "John Doe",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1985, 5, 20),
                    Email = "john.doe@example.com",
                    Phone = "123-456-7890",
                    Address = "123 Main St, Anytown, USA",
                    Photo = null
                },
                new CreateBusinessCardDto
                {
                    Name = "Jane Smith",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1990, 9, 15),
                    Email = "jane.smith@example.com",
                    Phone = "987-654-3210",
                    Address = "456 Oak Ave, Somecity, USA",
                    Photo = null
                },
                new CreateBusinessCardDto
                {
                    Name = "Alex Ray",
                    Gender = "Other",
                    DateOfBirth = new DateTime(1992, 11, 30),
                    Email = "alex.ray@example.com",
                    Phone = "555-555-5555",
                    Address = "789 Pine Ln, Yourtown, USA",
                    Photo = null
                }
            };

            foreach (var dto in samples)
            {
                try
                {
                    await _service.AddAsync(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to add sample business card {Name}", dto.Name);
                }
            }

            _logger.LogInformation("Database seeding completed. Added {Count} sample cards.", samples.Length);
        }
    }
}