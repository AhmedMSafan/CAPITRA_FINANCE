using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingRollbackRunConfiguration : IEntityTypeConfiguration<GlPostingRollbackRun>
{
    public void Configure(EntityTypeBuilder<GlPostingRollbackRun> builder)
    {
        builder.ToTable("GL_POSTING_ROLLBACK_RUNS");
        builder.HasKey(x => x.RollbackRunId);
        builder.HasIndex(x => x.RollbackRunNo).IsUnique();
        builder.HasIndex(x => new { x.PostingRunId, x.RollbackStatus });

        builder.Property(x => x.RollbackRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RollbackRunNo).HasMaxLength(50);
        builder.Property(x => x.RollbackReasonCode).HasMaxLength(50);
        builder.Property(x => x.RollbackReason).HasMaxLength(1000);
        builder.Property(x => x.RollbackStatus).HasMaxLength(30);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
