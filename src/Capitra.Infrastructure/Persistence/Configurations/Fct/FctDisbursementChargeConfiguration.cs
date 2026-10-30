using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctDisbursementChargeConfiguration : IEntityTypeConfiguration<FctDisbursementCharge>
{
    public void Configure(EntityTypeBuilder<FctDisbursementCharge> builder)
    {
        builder.ToTable("FCT_DISBURSEMENT_CHARGES");
        builder.HasKey(x => x.DisbursementChargeId);
        builder.HasIndex(x => new { x.DisbursementId, x.RuleChargeId }).IsUnique();
        builder.HasIndex(x => new { x.DisbursementId, x.StatusCode });

        builder.Property(x => x.DisbursementChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(80);
        builder.Property(x => x.RuleName).HasMaxLength(200);
        builder.Property(x => x.CalculationMethod).HasMaxLength(25);
        builder.Property(x => x.BaseCode).HasMaxLength(80);
        builder.Property(x => x.BaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.Units).HasPrecision(20, 6);
        builder.Property(x => x.FixedAmount).HasPrecision(18, 2);
        builder.Property(x => x.RatePercent).HasPrecision(18, 8);
        builder.Property(x => x.PerUnitAmount).HasPrecision(18, 2);
        builder.Property(x => x.SpreadPercent).HasPrecision(18, 8);
        builder.Property(x => x.ChargeAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxRatePercent).HasPrecision(9, 6);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChargeTimingCode).HasMaxLength(25);
        builder.Property(x => x.IncomeRecognitionCode).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.CollectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.WaivedAmount).HasPrecision(18, 2);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmProductEventRule>().WithMany().HasForeignKey(x => x.ProductEventRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmProductEventRuleCharge>().WithMany().HasForeignKey(x => x.RuleChargeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
