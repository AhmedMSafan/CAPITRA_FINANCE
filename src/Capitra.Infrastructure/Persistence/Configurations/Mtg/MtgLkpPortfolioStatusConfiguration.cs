using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpPortfolioStatusConfiguration : IEntityTypeConfiguration<MtgLkpPortfolioStatus>
{
    public void Configure(EntityTypeBuilder<MtgLkpPortfolioStatus> builder)
    {
        builder.ToTable("MTG_LKP_PORTFOLIO_STATUSES");
        builder.HasKey(x => x.StatusCode);

        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.StatusNameAr).HasMaxLength(200);
        builder.Property(x => x.StatusNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
