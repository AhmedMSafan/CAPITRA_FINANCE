using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseProfileStepConfiguration : IEntityTypeConfiguration<GlCloseProfileStep>
{
    public void Configure(EntityTypeBuilder<GlCloseProfileStep> builder)
    {
        builder.ToTable("GL_CLOSE_PROFILE_STEPS");
        builder.HasKey(x => x.CloseStepId);
        builder.HasIndex(x => new { x.CloseProfileId, x.StepCode }).IsUnique();
        builder.HasIndex(x => new { x.CloseProfileId, x.StepGroupCode, x.DisplaySeq });

        builder.Property(x => x.CloseStepId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepNameEn).HasMaxLength(200);
        builder.Property(x => x.StepNameAr).HasMaxLength(200);
        builder.Property(x => x.StepGroupCode).HasMaxLength(50);
        builder.Property(x => x.StepType).HasMaxLength(30);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.DependencyStepCode).HasMaxLength(50);
        builder.Property(x => x.ValidationSetCode).HasMaxLength(50);
        builder.Property(x => x.ResponsibleRoleCode).HasMaxLength(100);
        builder.Property(x => x.ResponsibleUserName).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlCloseProfile>().WithMany().HasForeignKey(x => x.CloseProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
