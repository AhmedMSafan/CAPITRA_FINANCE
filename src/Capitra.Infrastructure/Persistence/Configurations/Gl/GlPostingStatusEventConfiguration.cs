using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingStatusEventConfiguration : IEntityTypeConfiguration<GlPostingStatusEvent>
{
    public void Configure(EntityTypeBuilder<GlPostingStatusEvent> builder)
    {
        builder.ToTable("GL_POSTING_STATUS_EVENTS");
        builder.HasKey(x => x.PostingStatusEventId);
        builder.HasIndex(x => new { x.PostingRunId, x.EventAt });
        builder.HasIndex(x => new { x.QueueId, x.EventAt });
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentId, x.EventAt });

        builder.Property(x => x.PostingStatusEventId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(100);
        builder.Property(x => x.OldStatus).HasMaxLength(30);
        builder.Property(x => x.NewStatus).HasMaxLength(30);
        builder.Property(x => x.EventCode).HasMaxLength(50);
        builder.Property(x => x.EventMessage).HasMaxLength(4000);
        builder.Property(x => x.EventBy).HasMaxLength(100);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
