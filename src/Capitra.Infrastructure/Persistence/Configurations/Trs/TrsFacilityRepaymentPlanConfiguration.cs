using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsFacilityRepaymentPlanConfiguration : IEntityTypeConfiguration<TrsFacilityRepaymentPlan>
{
    public void Configure(EntityTypeBuilder<TrsFacilityRepaymentPlan> builder)
    {
        builder.ToTable("TRS_FACILITY_REPAYMENT_PLANS");
        builder.HasKey(x => x.RepaymentPlanId);
        builder.HasIndex(x => new { x.RunoffCaseId, x.InstallmentNo }).IsUnique();
        builder.HasIndex(x => new { x.RunoffCaseId, x.ContractualDueDate });
        builder.HasIndex(x => new { x.FacilityId, x.BankCollectionDate });
        builder.HasIndex(x => new { x.DrawdownId, x.BankCollectionDate });

        builder.Property(x => x.RepaymentPlanId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OpeningBalance).HasPrecision(18, 3);
        builder.Property(x => x.PrincipalDueAmount).HasPrecision(18, 2);
        builder.Property(x => x.InterestDueAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeeDueAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalDueAmount).HasPrecision(18, 2);
        builder.Property(x => x.ClosingBalance).HasPrecision(18, 3);
        builder.Property(x => x.PlanSourceCode).HasMaxLength(30);
        builder.Property(x => x.PlanStatusCode).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.AnnualRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ClientInterestRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ClientInterestDueAmount).HasPrecision(18, 2);

        // RUNOFF_CASE_ID -> TRS_FACILITY_RUNOFF_CASES: added when that table is ported.
        builder.HasOne<TrsCreditFacility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        // DRAWDOWN_ID -> TRS_FACILITY_DRAWDOWNS: added when that table is ported.
        // REPAYMENT_RULE_ID -> TRS_FACILITY_REPAYMENT_RULES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
