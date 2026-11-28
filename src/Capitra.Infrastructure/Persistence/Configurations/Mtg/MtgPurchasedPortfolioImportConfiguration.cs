using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioImportConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioImport>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioImport> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_IMPORTS");
        builder.HasKey(x => x.StagingId);
        builder.HasIndex(x => new { x.PortfolioId, x.WfInstanceId }).IsUnique();

        builder.Property(x => x.StagingId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.TotalRows).HasPrecision(28, 2);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Cascade);
        // WF_INSTANCE_ID -> WFE_WORKFLOW_INSTANCES: added when the WFE tables are ported.
        builder.ConfigureAudit();
    }
}
