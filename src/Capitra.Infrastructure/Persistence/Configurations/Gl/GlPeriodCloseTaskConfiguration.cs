using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodCloseTaskConfiguration : IEntityTypeConfiguration<GlPeriodCloseTask>
{
    public void Configure(EntityTypeBuilder<GlPeriodCloseTask> builder)
    {
        builder.ToTable("GL_PERIOD_CLOSE_TASKS");
        builder.HasKey(x => x.CloseTaskId);
        builder.HasIndex(x => x.TaskCode).IsUnique();
        builder.HasIndex(x => new { x.TaskArea, x.IsActive, x.DisplaySeq });

        builder.Property(x => x.CloseTaskId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TaskCode).HasMaxLength(60);
        builder.Property(x => x.TaskNameEn).HasMaxLength(200);
        builder.Property(x => x.TaskNameAr).HasMaxLength(200);
        builder.Property(x => x.TaskArea).HasMaxLength(30);
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.AutoCheckPackage).HasMaxLength(128);
        builder.Property(x => x.AutoCheckProcedure).HasMaxLength(128);
        builder.Property(x => x.DefaultResponsibleRole).HasMaxLength(60);

        builder.ConfigureAudit();
    }
}
