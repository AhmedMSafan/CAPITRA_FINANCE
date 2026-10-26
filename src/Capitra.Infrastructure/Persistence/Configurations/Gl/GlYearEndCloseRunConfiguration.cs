using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlYearEndCloseRunConfiguration : IEntityTypeConfiguration<GlYearEndCloseRun>
{
    public void Configure(EntityTypeBuilder<GlYearEndCloseRun> builder)
    {
        builder.ToTable("GL_YEAR_END_CLOSE_RUNS");
        builder.HasKey(x => x.YearEndRunId);
        builder.HasIndex(x => new { x.LedgerId, x.FiscalYearId }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.FiscalYearId, x.RunStatus });

        builder.Property(x => x.YearEndRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.RevenueTotal).HasPrecision(28, 2);
        builder.Property(x => x.ExpenseTotal).HasPrecision(28, 2);
        builder.Property(x => x.BalanceSheetTotalDr).HasPrecision(28, 2);
        builder.Property(x => x.BalanceSheetTotalCr).HasPrecision(28, 2);
        builder.Property(x => x.OpeningBalanceTotalDr).HasPrecision(28, 2);
        builder.Property(x => x.OpeningBalanceTotalCr).HasPrecision(28, 2);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.PostedBy).HasMaxLength(100);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.TargetFiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.RetainedEarningsAccountCombId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.OpeningBalanceBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.ClosingJournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
