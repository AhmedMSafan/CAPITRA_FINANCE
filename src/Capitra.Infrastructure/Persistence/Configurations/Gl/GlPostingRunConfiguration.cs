using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingRunConfiguration : IEntityTypeConfiguration<GlPostingRun>
{
    public void Configure(EntityTypeBuilder<GlPostingRun> builder)
    {
        builder.ToTable("GL_POSTING_RUNS");
        builder.HasKey(x => x.PostingRunId);
        builder.HasIndex(x => new { x.LedgerId, x.PostingRunNo }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.RunStatus });

        builder.Property(x => x.PostingRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PostingRunNo).HasMaxLength(80);
        builder.Property(x => x.PostingRunType).HasMaxLength(30);
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.StartedBy).HasMaxLength(255);
        builder.Property(x => x.TotalHeaders).HasPrecision(28, 2);
        builder.Property(x => x.TotalLines).HasPrecision(28, 2);
        builder.Property(x => x.TotalAccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.TotalAccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.BalanceUpdateMode).HasMaxLength(30);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);

        builder.HasOne<GlPostingProfile>().WithMany().HasForeignKey(x => x.PostingProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.SubmittedBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
