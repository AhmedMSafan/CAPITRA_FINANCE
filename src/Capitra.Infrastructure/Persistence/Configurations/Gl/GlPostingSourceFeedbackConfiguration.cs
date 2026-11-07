using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingSourceFeedbackConfiguration : IEntityTypeConfiguration<GlPostingSourceFeedback>
{
    public void Configure(EntityTypeBuilder<GlPostingSourceFeedback> builder)
    {
        builder.ToTable("GL_POSTING_SOURCE_FEEDBACKS");
        builder.HasKey(x => x.SourceFeedbackId);
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentId, x.SourceTransactionId, x.JournalHeaderId }).IsUnique();
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentId, x.PostingStatus });
        builder.HasIndex(x => new { x.FeedbackStatus, x.LastAttemptAt });

        builder.Property(x => x.SourceFeedbackId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(100);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(100);
        builder.Property(x => x.SourceTransactionId).HasMaxLength(100);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.GlJournalNo).HasMaxLength(100);
        builder.Property(x => x.FeedbackStatus).HasMaxLength(30);
        builder.Property(x => x.LastErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
