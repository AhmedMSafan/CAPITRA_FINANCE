using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseExceptionConfiguration : IEntityTypeConfiguration<GlCloseException>
{
    public void Configure(EntityTypeBuilder<GlCloseException> builder)
    {
        builder.ToTable("GL_CLOSE_EXCEPTIONS");
        builder.HasKey(x => x.CloseExceptionId);
        builder.HasIndex(x => new { x.CloseRunId, x.ExceptionStatus, x.Severity });

        builder.Property(x => x.CloseExceptionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ExceptionCode).HasMaxLength(50);
        builder.Property(x => x.ExceptionTitle).HasMaxLength(300);
        builder.Property(x => x.ExceptionDescription).HasMaxLength(2000);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.ExceptionStatus).HasMaxLength(30);
        builder.Property(x => x.AssignedTo).HasMaxLength(100);
        builder.Property(x => x.ResolvedBy).HasMaxLength(100);
        builder.Property(x => x.ResolutionNotes).HasMaxLength(1000);
        builder.Property(x => x.WaiverApprovedBy).HasMaxLength(100);
        builder.Property(x => x.WaiverReason).HasMaxLength(1000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.CloseRunTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseValidationResult>().WithMany().HasForeignKey(x => x.ValidationResultId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
