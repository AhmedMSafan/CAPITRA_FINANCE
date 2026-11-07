using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmRateIndexValueConfiguration : IEntityTypeConfiguration<AdmRateIndexValue>
{
    public void Configure(EntityTypeBuilder<AdmRateIndexValue> builder)
    {
        builder.ToTable("ADM_RATE_INDEX_VALUES");
        builder.HasKey(x => x.RateValueId);
        builder.HasIndex(x => new { x.RateIndexId, x.EffectiveFromDate }).IsUnique();
        builder.HasIndex(x => new { x.RateIndexId, x.EffectiveFromDate, x.EffectiveToDate });

        builder.Property(x => x.RateValueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AnnualRatePct).HasPrecision(9, 6);

        builder.HasOne<AdmRateIndex>().WithMany().HasForeignKey(x => x.RateIndexId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
