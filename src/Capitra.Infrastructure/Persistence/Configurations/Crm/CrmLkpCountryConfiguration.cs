using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmLkpCountryConfiguration : IEntityTypeConfiguration<CrmLkpCountry>
{
    public void Configure(EntityTypeBuilder<CrmLkpCountry> builder)
    {
        builder.ToTable("CRM_LKP_COUNTRIES");
        builder.HasKey(x => x.CountryId);
        builder.HasIndex(x => x.CountryCodeA3).IsUnique();
        builder.HasIndex(x => x.CountryCode).IsUnique();

        builder.Property(x => x.CountryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CountryCode).HasMaxLength(3);
        builder.Property(x => x.CountryCodeA3).HasMaxLength(3);
        builder.Property(x => x.CountryName).HasMaxLength(200);
        builder.Property(x => x.CountryNameAr).HasMaxLength(200);
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.NationalityAr).HasMaxLength(100);
        builder.Property(x => x.PhoneCode).HasMaxLength(10);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Continent).HasMaxLength(50);
        builder.Property(x => x.ContinentAr).HasMaxLength(50);
        builder.Property(x => x.RiskRating).HasMaxLength(10);

        builder.ConfigureAudit();
    }
}
