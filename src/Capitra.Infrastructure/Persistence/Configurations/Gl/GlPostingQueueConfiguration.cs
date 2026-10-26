using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingQueueConfiguration : IEntityTypeConfiguration<GlPostingQueue>
{
    public void Configure(EntityTypeBuilder<GlPostingQueue> builder)
    {
        builder.ToTable("GL_POSTING_QUEUES");
        builder.HasKey(x => x.QueueId);
        builder.HasIndex(x => x.QueueNo).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.RequestStatus, x.RequestPriority });
        builder.HasIndex(x => new { x.JournalHeaderId, x.RequestStatus });
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentType, x.SourceDocumentId, x.SourceTransactionId });
        builder.HasIndex(x => x.PostingRunId);

        builder.Property(x => x.QueueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.QueueNo).HasMaxLength(50);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(100);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(100);
        builder.Property(x => x.SourceTransactionId).HasMaxLength(100);
        builder.Property(x => x.RequestStatus).HasMaxLength(30);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.HoldReason).HasMaxLength(1000);
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingProfile>().WithMany().HasForeignKey(x => x.PostingProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
