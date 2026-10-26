using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmClientConfiguration : IEntityTypeConfiguration<CrmClient>
{
    public void Configure(EntityTypeBuilder<CrmClient> builder)
    {
        builder.ToTable("CRM_CLIENTS");
        builder.HasKey(x => x.ClientId);
        builder.HasIndex(x => x.CityId);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.GroupCode);
        builder.HasIndex(x => x.IndustryId);
        builder.HasIndex(x => new { x.KycStatus, x.KycExpiryDate });
        builder.HasIndex(x => x.ParentClientId);
        builder.HasIndex(x => x.RmEmployeeId);
        builder.HasIndex(x => x.SectorId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.TaxNumber);
        builder.HasIndex(x => x.ClientNumber).IsUnique();

        builder.Property(x => x.ClientId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ClientNumber).HasMaxLength(20);
        builder.Property(x => x.ClientRefCode).HasMaxLength(30);
        builder.Property(x => x.ClientName).HasMaxLength(500);
        builder.Property(x => x.ClientNameAr).HasMaxLength(500);
        builder.Property(x => x.ShortName).HasMaxLength(100);
        builder.Property(x => x.ShortNameAr).HasMaxLength(100);
        builder.Property(x => x.TradeName).HasMaxLength(500);
        builder.Property(x => x.TradeNameAr).HasMaxLength(500);
        builder.Property(x => x.ClientType).HasMaxLength(30);
        builder.Property(x => x.LegalStructure).HasMaxLength(50);
        builder.Property(x => x.LegalStructureAr).HasMaxLength(50);
        builder.Property(x => x.RegistrationNumber).HasMaxLength(50);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.VatNumber).HasMaxLength(50);
        builder.Property(x => x.NationalId).HasMaxLength(30);
        builder.Property(x => x.UnifiedNumber).HasMaxLength(30);
        builder.Property(x => x.LicenseNumber).HasMaxLength(50);
        builder.Property(x => x.ClientCategory).HasMaxLength(30);
        builder.Property(x => x.ClientSegment).HasMaxLength(30);
        builder.Property(x => x.RiskGrade).HasMaxLength(20);
        builder.Property(x => x.AnnualTurnover).HasPrecision(18, 3);
        builder.Property(x => x.CapitalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Website).HasMaxLength(500);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.MobileNumber).HasMaxLength(30);
        builder.Property(x => x.FaxNumber).HasMaxLength(30);
        builder.Property(x => x.RelationshipManager).HasMaxLength(200);
        builder.Property(x => x.KycStatus).HasMaxLength(30);
        builder.Property(x => x.AmlStatus).HasMaxLength(30);
        builder.Property(x => x.BlacklistReason).HasMaxLength(1000);
        builder.Property(x => x.BlacklistReasonAr).HasMaxLength(1000);
        builder.Property(x => x.GroupCode).HasMaxLength(30);
        builder.Property(x => x.GroupName).HasMaxLength(300);
        builder.Property(x => x.GroupNameAr).HasMaxLength(300);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.Attribute1).HasMaxLength(500);
        builder.Property(x => x.Attribute2).HasMaxLength(500);
        builder.Property(x => x.Attribute3).HasMaxLength(500);
        builder.Property(x => x.Attribute4).HasMaxLength(500);
        builder.Property(x => x.Attribute5).HasMaxLength(500);
        builder.Property(x => x.SupplierBankAccount).HasMaxLength(100);

        // CITY_ID -> LKP_CITIES: added when the LKP tables are ported.
        builder.HasOne<CrmLkpCountry>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.BaseCurrencyId).OnDelete(DeleteBehavior.Restrict);
        // INDUSTRY_ID -> LKP_INDUSTRIES: added when the LKP tables are ported.
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ParentClientId).OnDelete(DeleteBehavior.Restrict);
        // REGION_ID -> LKP_REGIONS: added when the LKP tables are ported.
        // SECTOR_ID -> LKP_ECONOMIC_SECTORS: added when the LKP tables are ported.
        builder.HasOne<CrmLkpStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
