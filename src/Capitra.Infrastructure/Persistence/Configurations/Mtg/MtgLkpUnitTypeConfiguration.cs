using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpUnitTypeConfiguration : IEntityTypeConfiguration<MtgLkpUnitType>
{
    public void Configure(EntityTypeBuilder<MtgLkpUnitType> builder)
    {
        builder.ToTable("MTG_LKP_UNIT_TYPES");
        builder.HasKey(x => x.UnitTypeCode);

        builder.Property(x => x.UnitTypeCode).HasMaxLength(30);
        builder.Property(x => x.UnitTypeNameAr).HasMaxLength(100);
        builder.Property(x => x.UnitTypeNameEn).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
