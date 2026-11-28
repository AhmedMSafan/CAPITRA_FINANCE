using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingValidationResultConfiguration : IEntityTypeConfiguration<GlPostingValidationResult>
{
    public void Configure(EntityTypeBuilder<GlPostingValidationResult> builder)
    {
        builder.ToTable("GL_POSTING_VALIDATION_RESULTS");
        builder.HasKey(x => x.PostingValidationResultId);
        builder.HasIndex(x => new { x.PostingRunId, x.ResultStatus, x.SeverityCode });
        builder.HasIndex(x => new { x.QueueId, x.ResultStatus, x.SeverityCode });
        builder.HasIndex(x => new { x.JournalHeaderId, x.ResultStatus, x.SeverityCode });

        builder.Property(x => x.PostingValidationResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValidationStage).HasMaxLength(30);
        builder.Property(x => x.ResultStatus).HasMaxLength(30);
        builder.Property(x => x.SeverityCode).HasMaxLength(30);
        builder.Property(x => x.ResultMessage).HasMaxLength(4000);
        builder.Property(x => x.CheckedBy).HasMaxLength(100);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingValidationRule>().WithMany().HasForeignKey(x => x.PostingValidationRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
