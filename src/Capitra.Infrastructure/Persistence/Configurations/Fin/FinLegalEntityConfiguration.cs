using Capitra.Domain.Modules.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinLegalEntityConfiguration : IEntityTypeConfiguration<FinLegalEntity>
{
    public void Configure(EntityTypeBuilder<FinLegalEntity> builder)
    {
        builder.ToTable("FIN_LEGAL_ENTITIES");
        builder.HasKey(x => x.LegalEntityId);
        builder.HasIndex(x => x.LegalEntityCode).IsUnique();

        builder.Property(x => x.LegalEntityId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LegalEntityCode).HasMaxLength(30);
        builder.Property(x => x.LegalEntityNameEn).HasMaxLength(240);
        builder.Property(x => x.LegalEntityNameAr).HasMaxLength(240);
        builder.Property(x => x.ShortName).HasMaxLength(80);
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.BaseCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.TaxRegistrationNo).HasMaxLength(80);
        builder.Property(x => x.CommercialRegisterNo).HasMaxLength(80);
        builder.Property(x => x.RegulatorCode).HasMaxLength(80);
        builder.Property(x => x.LegalAddressEn).HasMaxLength(500);
        builder.Property(x => x.LegalAddressAr).HasMaxLength(500);
        builder.Property(x => x.PhoneNo).HasMaxLength(50);
        builder.Property(x => x.EmailAddress).HasMaxLength(150);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(250);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.LegalEntityType).HasMaxLength(30);
        builder.Property(x => x.OwnershipPercent).HasPrecision(7, 4);
        builder.Property(x => x.ConsolidationMethod).HasMaxLength(30);

        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.BaseCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
