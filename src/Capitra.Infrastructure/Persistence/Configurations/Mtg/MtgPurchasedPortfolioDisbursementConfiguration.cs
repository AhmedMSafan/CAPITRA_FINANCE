using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioDisbursementConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioDisbursement>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioDisbursement> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS");
        builder.HasKey(x => x.DisbursementId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();
        builder.HasIndex(x => x.TranSetId).IsUnique();

        builder.Property(x => x.DisbursementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TranSetId).HasMaxLength(60);
        builder.Property(x => x.BankGlAccount).HasMaxLength(30);
        builder.Property(x => x.PaymentRef).HasMaxLength(200);
        builder.Property(x => x.CurrencyCode).HasMaxLength(10);
        builder.Property(x => x.PvMethod).HasMaxLength(20);
        builder.Property(x => x.PvRate).HasPrecision(9, 6);
        builder.Property(x => x.FaceAmount).HasPrecision(18, 2);
        builder.Property(x => x.PvAmount).HasPrecision(18, 2);
        builder.Property(x => x.FundingRecoveryAmount).HasPrecision(18, 2);
        builder.Property(x => x.MarginAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminFrAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.LegalFrAmount).HasPrecision(18, 2);
        builder.Property(x => x.LegalFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ValuationFrAmount).HasPrecision(18, 2);
        builder.Property(x => x.ValuationFinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeesAmount).HasPrecision(18, 2);
        builder.Property(x => x.BoardPriceAmount).HasPrecision(18, 2);
        builder.Property(x => x.PayableAmount).HasPrecision(18, 2);
        builder.Property(x => x.GlBatchNo).HasMaxLength(240);
        builder.Property(x => x.GlStatus).HasMaxLength(30);
        builder.Property(x => x.Comments).HasMaxLength(4000);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.MarginTreatment).HasMaxLength(20);
        builder.Property(x => x.AdminTreatment).HasMaxLength(20);
        builder.Property(x => x.LegalTreatment).HasMaxLength(20);
        builder.Property(x => x.ValuationTreatment).HasMaxLength(20);
        builder.Property(x => x.MarginDeferredAmount).HasPrecision(18, 2);
        builder.Property(x => x.MarginImmediateAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeesDeferredAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeesImmediateAmount).HasPrecision(18, 2);

        builder.ConfigureAudit();
    }
}
