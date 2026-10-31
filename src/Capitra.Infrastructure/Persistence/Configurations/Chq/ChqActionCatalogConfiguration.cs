using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqActionCatalogConfiguration : IEntityTypeConfiguration<ChqActionCatalog>
{
    public void Configure(EntityTypeBuilder<ChqActionCatalog> builder)
    {
        builder.ToTable("CHQ_ACTION_CATALOGS");
        builder.HasKey(x => x.ActionCode);

        builder.Property(x => x.ActionCode).HasMaxLength(50);
        builder.Property(x => x.ActionNameEn).HasMaxLength(200);
        builder.Property(x => x.ActionNameAr).HasMaxLength(200);
        builder.Property(x => x.EventType).HasMaxLength(30);
        builder.Property(x => x.TargetLocationCode).HasMaxLength(50);
        builder.Property(x => x.AllowedFromStatus).HasMaxLength(500);
        builder.Property(x => x.AllowedFromLocation).HasMaxLength(500);
        builder.Property(x => x.PackageProcedureName).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
