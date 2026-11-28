using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmRateIndexConfiguration : IEntityTypeConfiguration<AdmRateIndex>
{
    public void Configure(EntityTypeBuilder<AdmRateIndex> builder)
    {
        builder.ToTable("ADM_RATE_INDEXES");
        builder.HasKey(x => x.RateIndexId);
        builder.HasIndex(x => x.RateIndexCode).IsUnique();

        builder.Property(x => x.RateIndexId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RateIndexCode).HasMaxLength(50);
        builder.Property(x => x.RateIndexName).HasMaxLength(200);

        // CURRENCY_ID -> FNV_CURRENCIES: added when the CAP tables are ported.
        builder.ConfigureAudit();
    }
}
