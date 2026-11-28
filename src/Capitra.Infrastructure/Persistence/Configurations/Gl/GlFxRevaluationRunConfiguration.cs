using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlFxRevaluationRunConfiguration : IEntityTypeConfiguration<GlFxRevaluationRun>
{
    public void Configure(EntityTypeBuilder<GlFxRevaluationRun> builder)
    {
        builder.ToTable("GL_FX_REVALUATION_RUNS");
        builder.HasKey(x => x.RevaluationRunId);
        builder.HasIndex(x => x.RunCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.RunStatus });

        builder.Property(x => x.RevaluationRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunCode).HasMaxLength(50);
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.TotalGainAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalLossAmount).HasPrecision(18, 2);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.PostedBy).HasMaxLength(100);
        builder.Property(x => x.ReversedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFxRevaluationRule>().WithMany().HasForeignKey(x => x.RevaluationRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
