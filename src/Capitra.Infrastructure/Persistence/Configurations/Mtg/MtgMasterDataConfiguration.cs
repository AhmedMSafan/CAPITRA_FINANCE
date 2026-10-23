using Capitra.Domain.Common;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

/// <summary>Master data of the portfolio purchasing module. Lengths follow the APEX form items.</summary>
public sealed class MtgDeveloperConfiguration : IEntityTypeConfiguration<MtgDeveloper>
{
    public void Configure(EntityTypeBuilder<MtgDeveloper> builder)
    {
        builder.ToTable("MTG_DEVELOPER");
        builder.HasKey(x => x.DeveloperId);
        builder.HasIndex(x => x.DeveloperCode).IsUnique();

        builder.Property(x => x.DeveloperCode).HasMaxLength(30);
        builder.Property(x => x.LegalNameAr).HasMaxLength(300);
        builder.Property(x => x.LegalNameEn).HasMaxLength(300);
        builder.Property(x => x.CommercialRegNo).HasMaxLength(100);
        builder.Property(x => x.TaxRegNo).HasMaxLength(100);
        builder.Property(x => x.AddressText).HasMaxLength(1000);
        builder.Property(x => x.PhoneNo).HasMaxLength(100);
        builder.Property(x => x.EmailAddress).HasMaxLength(320);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(LookupEntity.CodeMaxLength);

        builder.HasOne<AdmLkpRiskLevel>().WithMany().HasForeignKey(x => x.RiskLevelCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgDeveloperSignatoryConfiguration : IEntityTypeConfiguration<MtgDeveloperSignatory>
{
    public void Configure(EntityTypeBuilder<MtgDeveloperSignatory> builder)
    {
        builder.ToTable("MTG_DEVELOPER_SIGNATORY");
        builder.HasKey(x => x.DeveloperSignatoryId);

        builder.Property(x => x.SignatoryName).HasMaxLength(300);
        builder.Property(x => x.NationalIdNo).HasMaxLength(50);
        builder.Property(x => x.JobTitle).HasMaxLength(200);
        builder.Property(x => x.AuthorizationReference).HasMaxLength(300);

        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgProjectConfiguration : IEntityTypeConfiguration<MtgProject>
{
    public void Configure(EntityTypeBuilder<MtgProject> builder)
    {
        builder.ToTable("MTG_PROJECT");
        builder.HasKey(x => x.ProjectId);
        builder.HasIndex(x => x.ProjectCode).IsUnique();

        builder.Property(x => x.ProjectCode).HasMaxLength(30);
        builder.Property(x => x.ProjectNameAr).HasMaxLength(300);
        builder.Property(x => x.ProjectNameEn).HasMaxLength(300);
        builder.Property(x => x.LocationText).HasMaxLength(1000);
        builder.Property(x => x.LandArea).HasPrecision(18, 2);
        builder.Property(x => x.TitleDeedNo).HasMaxLength(100);
        builder.Property(x => x.BuildingLicenseNo).HasMaxLength(100);
        builder.Property(x => x.ProjectStatus).HasMaxLength(50);

        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgProjectUnitConfiguration : IEntityTypeConfiguration<MtgProjectUnit>
{
    public void Configure(EntityTypeBuilder<MtgProjectUnit> builder)
    {
        builder.ToTable("MTG_PROJECT_UNIT");
        builder.HasKey(x => x.ProjectUnitId);
        builder.HasIndex(x => new { x.ProjectId, x.UnitCode }).IsUnique();

        builder.Property(x => x.UnitCode).HasMaxLength(50);
        builder.Property(x => x.UnitTypeCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.BuildingNo).HasMaxLength(50);
        builder.Property(x => x.FloorNo).HasMaxLength(50);
        builder.Property(x => x.UnitNo).HasMaxLength(50);
        builder.Property(x => x.AreaSqm).HasPrecision(18, 2);
        builder.Property(x => x.LicenseStatus).HasMaxLength(50);

        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpUnitType>().WithMany().HasForeignKey(x => x.UnitTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgInvestorConfiguration : IEntityTypeConfiguration<MtgInvestor>
{
    public void Configure(EntityTypeBuilder<MtgInvestor> builder)
    {
        builder.ToTable("MTG_INVESTOR");
        builder.HasKey(x => x.InvestorId);
        builder.HasIndex(x => x.InvestorCode).IsUnique();

        builder.Property(x => x.InvestorCode).HasMaxLength(30);
        builder.Property(x => x.FullNameAr).HasMaxLength(300);
        builder.Property(x => x.FullNameEn).HasMaxLength(300);
        builder.Property(x => x.NationalIdNo).HasMaxLength(50);
        builder.Property(x => x.PhoneNo).HasMaxLength(100);
        builder.Property(x => x.EmailAddress).HasMaxLength(320);
        builder.Property(x => x.AddressText).HasMaxLength(1000);
        builder.Property(x => x.EmployerName).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
