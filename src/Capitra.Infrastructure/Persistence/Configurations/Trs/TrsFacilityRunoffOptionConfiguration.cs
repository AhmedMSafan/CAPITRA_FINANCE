using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsFacilityRunoffOptionConfiguration : IEntityTypeConfiguration<TrsFacilityRunoffOption>
{
    public void Configure(EntityTypeBuilder<TrsFacilityRunoffOption> builder)
    {
        builder.ToTable("TRS_FACILITY_RUNOFF_OPTIONS");
        builder.HasKey(x => x.RunoffOptionsId);
        builder.HasIndex(x => x.RunoffCaseId).IsUnique();

        builder.Property(x => x.RunoffOptionsId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InterestDayBasisCode).HasMaxLength(30);
        builder.Property(x => x.RepaymentFrequencyCode).HasMaxLength(30);
        builder.Property(x => x.AmortizationTypeCode).HasMaxLength(50);
        builder.Property(x => x.BankCollectionPolicyCode).HasMaxLength(30);
        builder.Property(x => x.AccrualFrequencyCode).HasMaxLength(30);
        builder.Property(x => x.RateModeCode).HasMaxLength(30);
        builder.Property(x => x.AdjustmentPostingModeCode).HasMaxLength(30);
        builder.Property(x => x.PreviewTargetCode).HasMaxLength(30);
        builder.Property(x => x.PreviewCollectionName).HasMaxLength(100);

        // RUNOFF_CASE_ID -> TRS_FACILITY_RUNOFF_CASES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
