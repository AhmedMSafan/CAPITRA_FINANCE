using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgValuationConfiguration : IEntityTypeConfiguration<MtgValuation>
{
    public void Configure(EntityTypeBuilder<MtgValuation> builder)
    {
        builder.ToTable("MTG_VALUATIONS");
        builder.HasKey(x => x.ValuationId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.ValuationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValuationTypeCode).HasMaxLength(30);
        builder.Property(x => x.ValuerName).HasMaxLength(300);
        builder.Property(x => x.ValuerRegistrationNo).HasMaxLength(100);
        builder.Property(x => x.MarketValue).HasPrecision(18, 2);
        builder.Property(x => x.RecommendedValue).HasPrecision(18, 2);
        builder.Property(x => x.ProjectCompletionPct).HasPrecision(9, 6);
        builder.Property(x => x.FacilitiesStatus).HasMaxLength(500);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolioDocument>().WithMany().HasForeignKey(x => x.ReportDocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpValuationType>().WithMany().HasForeignKey(x => x.ValuationTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProjectUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
