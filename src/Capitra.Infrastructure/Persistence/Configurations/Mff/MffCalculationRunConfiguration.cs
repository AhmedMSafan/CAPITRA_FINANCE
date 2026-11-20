using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffCalculationRunConfiguration : IEntityTypeConfiguration<MffCalculationRun>
{
    public void Configure(EntityTypeBuilder<MffCalculationRun> builder)
    {
        builder.ToTable("MFF_CALCULATION_RUNS");
        builder.HasKey(x => x.CalcRunId);
        builder.HasIndex(x => x.ProgramVersionId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.FinancierId);

        builder.Property(x => x.CalcRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunType).HasMaxLength(30);
        builder.Property(x => x.InputIncome).HasPrecision(18, 2);
        builder.Property(x => x.InputPropertyValue).HasPrecision(18, 2);
        builder.Property(x => x.InputDownpayment).HasPrecision(18, 2);
        builder.Property(x => x.MortgageRatePct).HasPrecision(9, 6);
        builder.Property(x => x.TermYears).HasPrecision(6, 2);
        builder.Property(x => x.PaymentGrowthYears).HasPrecision(6, 2);
        builder.Property(x => x.WageIncreasePct).HasPrecision(9, 6);
        builder.Property(x => x.MaxPaymentIncomePct).HasPrecision(9, 6);
        builder.Property(x => x.MinDownpaymentPct).HasPrecision(9, 6);
        builder.Property(x => x.MaxDownpaymentPct).HasPrecision(9, 6);
        builder.Property(x => x.MaxSubsidyAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountFactorPct).HasPrecision(9, 6);
        builder.Property(x => x.InputMonthlySubsidy).HasPrecision(18, 2);
        builder.Property(x => x.OutBasicMortgage).HasPrecision(18, 2);
        builder.Property(x => x.OutMaxMortgage).HasPrecision(18, 2);
        builder.Property(x => x.OutPvBuydownSubsidy).HasPrecision(18, 2);
        builder.Property(x => x.OutDownpaymentAssist).HasPrecision(18, 2);
        builder.Property(x => x.OutTotalSubsidy).HasPrecision(18, 2);
        builder.Property(x => x.OutMortgageLoan).HasPrecision(18, 2);
        builder.Property(x => x.OutInitialPayment).HasPrecision(18, 2);
        builder.Property(x => x.OutLtvPct).HasPrecision(9, 6);
        builder.Property(x => x.OutBorrowerDownpayment).HasPrecision(18, 2);
        builder.Property(x => x.OutMaxPropertyValue).HasPrecision(18, 2);
        builder.Property(x => x.OutPropertyValue).HasPrecision(18, 2);
        builder.Property(x => x.OutMinPropertyValue).HasPrecision(18, 2);
        builder.Property(x => x.WarningMessageAr).HasMaxLength(1000);
        builder.Property(x => x.CalcStatus).HasMaxLength(30);
        builder.Property(x => x.SourceReference).HasMaxLength(500);

        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
