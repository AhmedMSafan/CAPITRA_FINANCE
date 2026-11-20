using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctCreditLimitConfiguration : IEntityTypeConfiguration<FctCreditLimit>
{
    public void Configure(EntityTypeBuilder<FctCreditLimit> builder)
    {
        builder.ToTable("FCT_CREDIT_LIMITS");
        builder.HasKey(x => x.CreditLimitId);
        builder.HasIndex(x => x.AgrmtDebtorId);
        builder.HasIndex(x => x.AgreementId);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.DebtorId);
        builder.HasIndex(x => new { x.EffectiveAt, x.ExpiryAt });
        builder.HasIndex(x => x.LimitLevel);
        builder.HasIndex(x => x.ParentLimitId);
        builder.HasIndex(x => x.PreviousLimitId);
        builder.HasIndex(x => x.NextReviewDate);
        builder.HasIndex(x => x.RiskGrade);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => new { x.IsTemporary, x.TempExpiryDate });
        builder.HasIndex(x => x.LimitType);
        builder.HasIndex(x => x.LimitNumber).IsUnique();

        builder.Property(x => x.CreditLimitId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LimitNumber).HasMaxLength(30);
        builder.Property(x => x.LimitRef).HasMaxLength(50);
        builder.Property(x => x.LimitName).HasMaxLength(500);
        builder.Property(x => x.LimitNameAr).HasMaxLength(500);
        builder.Property(x => x.LimitLevel).HasMaxLength(30);
        builder.Property(x => x.LimitType).HasMaxLength(30);
        builder.Property(x => x.LimitTypeAr).HasMaxLength(30);
        builder.Property(x => x.LimitCategory).HasMaxLength(30);
        builder.Property(x => x.LimitCategoryAr).HasMaxLength(30);
        builder.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        builder.Property(x => x.RecommendedAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrentLimit).HasPrecision(18, 3);
        builder.Property(x => x.UtilizedAmount).HasPrecision(18, 2);
        builder.Property(x => x.AvailableAmount).HasPrecision(18, 3);
        builder.Property(x => x.PendingAmount).HasPrecision(18, 2);
        builder.Property(x => x.ReservedAmount).HasPrecision(18, 2);
        builder.Property(x => x.ExcessAmount).HasPrecision(18, 2);
        builder.Property(x => x.UtilizationPct).HasPrecision(9, 6);
        builder.Property(x => x.PeakUtilization).HasPrecision(18, 3);
        builder.Property(x => x.AvgUtilizationPct).HasPrecision(9, 6);
        builder.Property(x => x.SublimitAmount).HasPrecision(18, 2);
        builder.Property(x => x.SublimitType).HasMaxLength(30);
        builder.Property(x => x.SublimitTypeAr).HasMaxLength(30);
        builder.Property(x => x.RiskGrade).HasMaxLength(20);
        builder.Property(x => x.RiskGradeAr).HasMaxLength(20);
        builder.Property(x => x.RiskScore).HasPrecision(10, 4);
        builder.Property(x => x.PdPercentage).HasPrecision(10, 6);
        builder.Property(x => x.LgdPercentage).HasPrecision(10, 6);
        builder.Property(x => x.RiskWeightPct).HasPrecision(9, 6);
        builder.Property(x => x.ExpectedLoss).HasPrecision(18, 3);
        builder.Property(x => x.ProvisionAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProvisionPct).HasPrecision(9, 6);
        builder.Property(x => x.AdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MaxAdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ConcentrationLimitPct).HasPrecision(9, 6);
        builder.Property(x => x.CurrentConcentrationPct).HasPrecision(9, 6);
        builder.Property(x => x.InsuredAmount).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoveragePct).HasPrecision(9, 6);
        builder.Property(x => x.InsurancePolicyRef).HasMaxLength(100);
        builder.Property(x => x.ReviewFrequency).HasMaxLength(20);
        builder.Property(x => x.RequestedBy).HasMaxLength(200);
        builder.Property(x => x.RecommendedBy).HasMaxLength(200);
        builder.Property(x => x.ApprovedBy).HasMaxLength(200);
        builder.Property(x => x.ApprovalAuthority).HasMaxLength(200);
        builder.Property(x => x.ApprovalAuthorityAr).HasMaxLength(200);
        builder.Property(x => x.ApprovalReference).HasMaxLength(100);
        builder.Property(x => x.ApprovalNotes).HasMaxLength(2000);
        builder.Property(x => x.ApprovalNotesAr).HasMaxLength(2000);
        builder.Property(x => x.Attribute1).HasMaxLength(500);
        builder.Property(x => x.Attribute2).HasMaxLength(500);
        builder.Property(x => x.Attribute3).HasMaxLength(500);
        builder.Property(x => x.Attribute4).HasMaxLength(500);
        builder.Property(x => x.Attribute5).HasMaxLength(500);
        builder.Property(x => x.SpreadRate).HasPrecision(9, 6);

        builder.HasOne<FctAgreementDebtor>().WithMany().HasForeignKey(x => x.AgrmtDebtorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctAgreement>().WithMany().HasForeignKey(x => x.AgreementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        // DEBTOR_ID -> FCT_DEBTORS: added when that table is ported.
        builder.HasOne<FctCreditLimit>().WithMany().HasForeignKey(x => x.ParentLimitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctCreditLimit>().WithMany().HasForeignKey(x => x.PreviousLimitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
