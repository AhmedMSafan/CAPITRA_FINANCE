using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingBalanceImpactConfiguration : IEntityTypeConfiguration<GlPostingBalanceImpact>
{
    public void Configure(EntityTypeBuilder<GlPostingBalanceImpact> builder)
    {
        builder.ToTable("GL_POSTING_BALANCE_IMPACTS");
        builder.HasKey(x => x.BalanceImpactId);
        builder.HasIndex(x => new { x.PostingRunId, x.LedgerId, x.LedgerPeriodId });
        builder.HasIndex(x => new { x.JournalHeaderId, x.JournalLineId });
        builder.HasIndex(x => new { x.AccountCombinationId, x.LedgerPeriodId, x.CurrencyCode });
        builder.HasIndex(x => new { x.AccountingDate, x.LedgerId, x.AccountCombinationId });

        builder.Property(x => x.BalanceImpactId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredDr).HasPrecision(22, 5);
        builder.Property(x => x.EnteredCr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceBeforeDr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceBeforeCr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceAfterDr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceAfterCr).HasPrecision(22, 5);
        builder.Property(x => x.ImpactStatus).HasMaxLength(30);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingBalanceImpact>().WithMany().HasForeignKey(x => x.ReversalImpactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingRunLine>().WithMany().HasForeignKey(x => x.PostingRunLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
