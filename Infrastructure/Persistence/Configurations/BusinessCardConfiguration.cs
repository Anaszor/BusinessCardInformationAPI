using BusinessCardInformationAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessCardInformationAPI.Infrastructure.Persistence.Configurations
{
    public class BusinessCardConfiguration : IEntityTypeConfiguration<BusinessCard>
    {
        public void Configure(EntityTypeBuilder<BusinessCard> builder)
        {
            builder.ToTable("BusinessCards");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.Gender)
                   .HasMaxLength(10);

            builder.Property(b => b.Email)
                   .HasMaxLength(100);

            builder.Property(b => b.Phone)
                   .HasMaxLength(20);

            builder.Property(b => b.Photo)
                   .HasColumnType("nvarchar(max)");

            builder.Property(b => b.Address)
                   .HasMaxLength(200);
        }
    }
}
