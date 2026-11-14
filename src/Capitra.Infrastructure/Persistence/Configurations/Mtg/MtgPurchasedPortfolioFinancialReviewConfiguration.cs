using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioFinancialReviewConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioFinancialReview>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioFinancialReview> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");
        builder.HasKey(x => x.ReviewId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();

        builder.Property(x => x.ReviewId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PvMethod).HasMaxLength(20);
        builder.Property(x => x.PvRate).HasPrecision(9, 6);
        builder.Property(x => x.ChequesFaceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChequesPvAmount).HasPrecision(18, 2);
        builder.Property(x => x.MarginBase).HasMaxLength(30);
        builder.Property(x => x.MarginPercentage).HasPrecision(9, 4);
        builder.Property(x => x.MarginBaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.MarginAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminBase).HasMaxLength(30);
        builder.Property(x => x.AdminExpensePercent).HasPrecision(9, 4);
        builder.Property(x => x.AdminBaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminCalcAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.LegalFeePerUnit).HasPrecision(18, 2);
        builder.Property(x => x.LegalCalcAmount).HasPrecision(18, 2);
        builder.Property(x => x.LegalFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ValuationFeePerUnit).HasPrecision(18, 2);
        builder.Property(x => x.ValuationCalcAmount).HasPrecision(18, 2);
        builder.Property(x => x.ValuationFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalChargesAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProposedPurchaseValue).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedReturnAmount).HasPrecision(18, 2);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
