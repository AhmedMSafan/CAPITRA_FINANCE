using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAttachmentConfiguration : IEntityTypeConfiguration<GlCloseAttachment>
{
    public void Configure(EntityTypeBuilder<GlCloseAttachment> builder)
    {
        builder.ToTable("GL_CLOSE_ATTACHMENTS");
        builder.HasKey(x => x.CloseAttachmentId);

        builder.Property(x => x.CloseAttachmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.AttachmentCategory).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.UploadedBy).HasMaxLength(100);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.CloseRunTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseReport>().WithMany().HasForeignKey(x => x.CloseReportId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
