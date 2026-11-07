using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOpeningBalanceRunConfiguration : IEntityTypeConfiguration<GlOpeningBalanceRun>
{
    public void Configure(EntityTypeBuilder<GlOpeningBalanceRun> builder)
    {
        builder.ToTable("GL_OPENING_BALANCE_RUNS");
        builder.HasKey(x => x.OpeningBalanceRunId);
        builder.HasIndex(x => new { x.LedgerId, x.TargetFiscalYearId }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.TargetFiscalYearId, x.RunStatus });

        builder.Property(x => x.OpeningBalanceRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.TotalDr).HasPrecision(28, 2);
        builder.Property(x => x.TotalCr).HasPrecision(28, 2);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.PostedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlYearEndCloseRun>().WithMany().HasForeignKey(x => x.YearEndRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.TargetFiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.TargetLedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
