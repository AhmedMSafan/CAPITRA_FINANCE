using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlFxRevaluationLineConfiguration : IEntityTypeConfiguration<GlFxRevaluationLine>
{
    public void Configure(EntityTypeBuilder<GlFxRevaluationLine> builder)
    {
        builder.ToTable("GL_FX_REVALUATION_LINES");
        builder.HasKey(x => x.RevaluationLineId);
        builder.HasIndex(x => new { x.RevaluationRunId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.RevaluationRunId, x.LineStatus });
        builder.HasIndex(x => new { x.CurrencyCode, x.AccountCombinationId });

        builder.Property(x => x.RevaluationLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredBalance).HasPrecision(24, 6);
        builder.Property(x => x.AccountedBalance).HasPrecision(24, 6);
        builder.Property(x => x.PeriodEndRate).HasPrecision(9, 6);
        builder.Property(x => x.RevaluedBalance).HasPrecision(24, 6);
        builder.Property(x => x.GainLossAmount).HasPrecision(18, 2);
        builder.Property(x => x.GainLossType).HasMaxLength(10);
        builder.Property(x => x.SourceReferenceType).HasMaxLength(30);
        builder.Property(x => x.SourceReferenceId).HasMaxLength(100);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.GainLossAccountCombId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFxRevaluationRun>().WithMany().HasForeignKey(x => x.RevaluationRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
