using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingExceptionConfiguration : IEntityTypeConfiguration<GlPostingException>
{
    public void Configure(EntityTypeBuilder<GlPostingException> builder)
    {
        builder.ToTable("GL_POSTING_EXCEPTIONS");
        builder.HasKey(x => x.PostingExceptionId);
        builder.HasIndex(x => new { x.PostingRunId, x.QueueId });
        builder.HasIndex(x => new { x.JournalHeaderId, x.JournalLineId });

        builder.Property(x => x.PostingExceptionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ExceptionCode).HasMaxLength(100);
        builder.Property(x => x.ExceptionType).HasMaxLength(30);
        builder.Property(x => x.SeverityCode).HasMaxLength(30);
        builder.Property(x => x.ExceptionMessage).HasMaxLength(4000);
        builder.Property(x => x.ResolutionStatus).HasMaxLength(30);
        builder.Property(x => x.AssignedTo).HasMaxLength(100);
        builder.Property(x => x.ResolvedBy).HasMaxLength(100);
        builder.Property(x => x.ResolutionNotes).HasMaxLength(4000);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
