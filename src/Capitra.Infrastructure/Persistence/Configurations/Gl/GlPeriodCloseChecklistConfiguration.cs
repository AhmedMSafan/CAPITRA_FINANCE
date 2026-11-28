using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodCloseChecklistConfiguration : IEntityTypeConfiguration<GlPeriodCloseChecklist>
{
    public void Configure(EntityTypeBuilder<GlPeriodCloseChecklist> builder)
    {
        builder.ToTable("GL_PERIOD_CLOSE_CHECKLISTS");
        builder.HasKey(x => x.CloseChecklistId);
        builder.HasIndex(x => new { x.LedgerPeriodId, x.CloseTaskId }).IsUnique();
        builder.HasIndex(x => new { x.LedgerPeriodId, x.TaskStatus });
        builder.HasIndex(x => new { x.CloseTaskId, x.TaskStatus });

        builder.Property(x => x.CloseChecklistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TaskStatus).HasMaxLength(30);
        builder.Property(x => x.ResponsibleRoleCode).HasMaxLength(60);
        builder.Property(x => x.AssignedToUser).HasMaxLength(100);
        builder.Property(x => x.CompletedBy).HasMaxLength(100);
        builder.Property(x => x.WaivedBy).HasMaxLength(100);
        builder.Property(x => x.WaiverReason).HasMaxLength(1000);
        builder.Property(x => x.ValidationMessage).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPeriodCloseTask>().WithMany().HasForeignKey(x => x.CloseTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
