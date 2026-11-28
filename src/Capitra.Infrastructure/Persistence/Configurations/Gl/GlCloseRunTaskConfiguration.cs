using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseRunTaskConfiguration : IEntityTypeConfiguration<GlCloseRunTask>
{
    public void Configure(EntityTypeBuilder<GlCloseRunTask> builder)
    {
        builder.ToTable("GL_CLOSE_RUN_TASKS");
        builder.HasKey(x => x.CloseRunTaskId);
        builder.HasIndex(x => new { x.CloseRunId, x.TaskStatus, x.DisplaySeq });

        builder.Property(x => x.CloseRunTaskId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.TaskNameEn).HasMaxLength(200);
        builder.Property(x => x.TaskNameAr).HasMaxLength(200);
        builder.Property(x => x.TaskGroupCode).HasMaxLength(50);
        builder.Property(x => x.TaskType).HasMaxLength(30);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.OwnerRoleCode).HasMaxLength(100);
        builder.Property(x => x.OwnerUserName).HasMaxLength(100);
        builder.Property(x => x.TaskStatus).HasMaxLength(30);
        builder.Property(x => x.CompletedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.WaiverReason).HasMaxLength(1000);
        builder.Property(x => x.CompletionNotes).HasMaxLength(1000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseProfileStep>().WithMany().HasForeignKey(x => x.CloseStepId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.DependencyTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
