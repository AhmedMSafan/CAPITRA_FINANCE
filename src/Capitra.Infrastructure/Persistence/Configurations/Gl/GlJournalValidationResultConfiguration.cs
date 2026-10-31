using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalValidationResultConfiguration : IEntityTypeConfiguration<GlJournalValidationResult>
{
    public void Configure(EntityTypeBuilder<GlJournalValidationResult> builder)
    {
        builder.ToTable("GL_JOURNAL_VALIDATION_RESULTS");
        builder.HasKey(x => x.ValidationResultId);

        builder.Property(x => x.ValidationResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.ResultStatus).HasMaxLength(30);
        builder.Property(x => x.ResultMessage).HasMaxLength(4000);
        builder.Property(x => x.ResolvedBy).HasMaxLength(255);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalValidationRule>().WithMany().HasForeignKey(x => x.ValidationRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
