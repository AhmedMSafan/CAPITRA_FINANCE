using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalAttachmentConfiguration : IEntityTypeConfiguration<GlJournalAttachment>
{
    public void Configure(EntityTypeBuilder<GlJournalAttachment> builder)
    {
        builder.ToTable("GL_JOURNAL_ATTACHMENTS");
        builder.HasKey(x => x.AttachmentId);

        builder.Property(x => x.AttachmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.MimeType).HasMaxLength(255);
        builder.Property(x => x.FileUrl).HasMaxLength(1000);
        builder.Property(x => x.AttachmentCategory).HasMaxLength(60);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
