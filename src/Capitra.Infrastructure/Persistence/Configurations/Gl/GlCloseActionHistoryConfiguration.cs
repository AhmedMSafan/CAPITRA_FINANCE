using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseActionHistoryConfiguration : IEntityTypeConfiguration<GlCloseActionHistory>
{
    public void Configure(EntityTypeBuilder<GlCloseActionHistory> builder)
    {
        builder.ToTable("GL_CLOSE_ACTION_HIST");
        builder.HasKey(x => x.CloseActionId);
        builder.HasIndex(x => new { x.CloseRunId, x.ActionAt });

        builder.Property(x => x.CloseActionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(50);
        builder.Property(x => x.ActionName).HasMaxLength(200);
        builder.Property(x => x.ActionFromStatus).HasMaxLength(50);
        builder.Property(x => x.ActionToStatus).HasMaxLength(50);
        builder.Property(x => x.ActionBy).HasMaxLength(100);
        builder.Property(x => x.ActionReason).HasMaxLength(1000);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.ReferenceType).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.CloseRunTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
