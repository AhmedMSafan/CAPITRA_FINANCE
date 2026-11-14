using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlSourceFeedbackConfiguration : IEntityTypeConfiguration<GlOracleGlSourceFeedback>
{
    public void Configure(EntityTypeBuilder<GlOracleGlSourceFeedback> builder)
    {
        builder.ToTable("GL_ORACLE_GL_SOURCE_FEEDBACKS");
        builder.HasKey(x => x.FeedbackId);
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentType, x.SourceDocumentId });

        builder.Property(x => x.FeedbackId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(50);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(100);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(100);
        builder.Property(x => x.OracleStatus).HasMaxLength(30);
        builder.Property(x => x.OracleReference).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
