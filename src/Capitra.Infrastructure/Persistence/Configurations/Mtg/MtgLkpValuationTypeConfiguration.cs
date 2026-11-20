using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpValuationTypeConfiguration : IEntityTypeConfiguration<MtgLkpValuationType>
{
    public void Configure(EntityTypeBuilder<MtgLkpValuationType> builder)
    {
        builder.ToTable("MTG_LKP_VALUATION_TYPES");
        builder.HasKey(x => x.ValuationTypeCode);

        builder.Property(x => x.ValuationTypeCode).HasMaxLength(30);
        builder.Property(x => x.ValuationNameAr).HasMaxLength(200);
        builder.Property(x => x.ValuationNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
