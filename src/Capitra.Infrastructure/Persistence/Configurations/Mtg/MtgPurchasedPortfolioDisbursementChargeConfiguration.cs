using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioDisbursementChargeConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioDisbursementCharge>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioDisbursementCharge> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES");
        builder.HasKey(x => x.DisbursementChargeId);
        builder.HasIndex(x => new { x.DisbursementId, x.ChargeCode }).IsUnique();
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.DisbursementChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChargeCode).HasMaxLength(50);
        builder.Property(x => x.ChargeNameEn).HasMaxLength(200);
        builder.Property(x => x.ChargeNameAr).HasMaxLength(200);
        builder.Property(x => x.CalculationMethodCode).HasMaxLength(30);
        builder.Property(x => x.AmountBaseCode).HasMaxLength(30);
        builder.Property(x => x.BaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChargeRate).HasPrecision(9, 6);
        builder.Property(x => x.PerUnitAmount).HasPrecision(18, 2);
        builder.Property(x => x.CalculatedAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.IncomeRecognitionCode).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolioDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
