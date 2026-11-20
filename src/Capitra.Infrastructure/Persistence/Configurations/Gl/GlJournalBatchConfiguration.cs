using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalBatchConfiguration : IEntityTypeConfiguration<GlJournalBatch>
{
    public void Configure(EntityTypeBuilder<GlJournalBatch> builder)
    {
        builder.ToTable("GL_JOURNAL_BATCHES");
        builder.HasKey(x => x.BatchId);
        builder.HasIndex(x => new { x.LedgerId, x.BatchNo }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.BatchStatus });
        builder.HasIndex(x => new { x.SourceId, x.SourceBatchRef });

        builder.Property(x => x.BatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchNo).HasMaxLength(60);
        builder.Property(x => x.BatchName).HasMaxLength(240);
        builder.Property(x => x.BatchStatus).HasMaxLength(30);
        builder.Property(x => x.ApprovalStatus).HasMaxLength(30);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.ControlTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.ControlTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.ActualTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.ActualTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.SourceBatchRef).HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.SubmittedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);
        builder.Property(x => x.PostedBy).HasMaxLength(255);
        builder.Property(x => x.CancelledBy).HasMaxLength(255);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
