using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccrualRunConfiguration : IEntityTypeConfiguration<GlCloseAccrualRun>
{
    public void Configure(EntityTypeBuilder<GlCloseAccrualRun> builder)
    {
        builder.ToTable("GL_CLOSE_ACCRUAL_RUNS");
        builder.HasKey(x => x.AccrualRunId);
        builder.HasIndex(x => new { x.CloseRunId, x.RunStatus });

        builder.Property(x => x.AccrualRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.TotalDebit).HasPrecision(28, 2);
        builder.Property(x => x.TotalCredit).HasPrecision(28, 2);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.PostedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseAccrualRule>().WithMany().HasForeignKey(x => x.AccrualRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.ReversalJournalId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
