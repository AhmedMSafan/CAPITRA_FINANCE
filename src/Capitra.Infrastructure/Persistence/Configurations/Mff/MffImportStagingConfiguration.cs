using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffImportStagingConfiguration : IEntityTypeConfiguration<MffImportStaging>
{
    public void Configure(EntityTypeBuilder<MffImportStaging> builder)
    {
        builder.ToTable("MFF_IMPORT_STAGINGS");
        builder.HasKey(x => x.StagingId);
        builder.HasIndex(x => new { x.BatchId, x.SourceRowNo }).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.BatchId);

        builder.Property(x => x.StagingId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ApplicantNameAr).HasMaxLength(300);
        builder.Property(x => x.ApplicantNationalId).HasMaxLength(30);
        builder.Property(x => x.SpouseNameAr).HasMaxLength(300);
        builder.Property(x => x.SpouseNationalId).HasMaxLength(30);
        builder.Property(x => x.MaritalStatusRaw).HasMaxLength(100);
        builder.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
        builder.Property(x => x.CustomerDownpayment).HasPrecision(18, 2);
        builder.Property(x => x.TotalSubsidy).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.MaintenanceDeposit).HasPrecision(18, 2);
        builder.Property(x => x.FinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.AnnouncementRaw).HasMaxLength(250);
        builder.Property(x => x.ProjectRaw).HasMaxLength(500);
        builder.Property(x => x.GovernorateRaw).HasMaxLength(200);
        builder.Property(x => x.CityRaw).HasMaxLength(200);
        builder.Property(x => x.DistrictRaw).HasMaxLength(200);
        builder.Property(x => x.AreaSqm).HasPrecision(10, 2);
        builder.Property(x => x.NeighborhoodRaw).HasMaxLength(200);
        builder.Property(x => x.BuildingNo).HasMaxLength(100);
        builder.Property(x => x.FloorNo).HasMaxLength(50);
        builder.Property(x => x.UnitNo).HasMaxLength(50);
        builder.Property(x => x.SiteRaw).HasMaxLength(1000);
        builder.Property(x => x.MobileNo).HasMaxLength(30);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffImportBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
