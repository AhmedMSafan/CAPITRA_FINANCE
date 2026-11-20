using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseCommentConfiguration : IEntityTypeConfiguration<GlCloseComment>
{
    public void Configure(EntityTypeBuilder<GlCloseComment> builder)
    {
        builder.ToTable("GL_CLOSE_COMMENTS");
        builder.HasKey(x => x.CloseCommentId);

        builder.Property(x => x.CloseCommentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CommentText).HasMaxLength(4000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.CloseRunTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseException>().WithMany().HasForeignKey(x => x.CloseExceptionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
