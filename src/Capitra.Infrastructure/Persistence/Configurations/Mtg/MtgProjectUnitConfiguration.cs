using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgProjectUnitConfiguration : IEntityTypeConfiguration<MtgProjectUnit>
{
    public void Configure(EntityTypeBuilder<MtgProjectUnit> builder)
    {
        builder.ToTable("MTG_PROJECT_UNITS");
        builder.HasKey(x => x.UnitId);
        builder.HasIndex(x => new { x.ProjectId, x.UnitCode }).IsUnique();
        builder.HasIndex(x => x.ProjectId);

        builder.Property(x => x.UnitId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UnitCode).HasMaxLength(50);
        builder.Property(x => x.UnitTypeCode).HasMaxLength(30);
        builder.Property(x => x.BuildingNo).HasMaxLength(50);
        builder.Property(x => x.FloorNo).HasMaxLength(50);
        builder.Property(x => x.UnitNo).HasMaxLength(50);
        builder.Property(x => x.AreaSqm).HasPrecision(18, 2);
        builder.Property(x => x.ContractValue).HasPrecision(18, 2);
        builder.Property(x => x.CurrentMarketValue).HasPrecision(18, 2);
        builder.Property(x => x.LicenseStatus).HasMaxLength(50);

        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpUnitType>().WithMany().HasForeignKey(x => x.UnitTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
