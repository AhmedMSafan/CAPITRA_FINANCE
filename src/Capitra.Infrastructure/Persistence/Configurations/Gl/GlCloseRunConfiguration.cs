using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseRunConfiguration : IEntityTypeConfiguration<GlCloseRun>
{
    public void Configure(EntityTypeBuilder<GlCloseRun> builder)
    {
        builder.ToTable("GL_CLOSE_RUNS");
        builder.HasKey(x => x.CloseRunId);
        builder.HasIndex(x => x.CloseRunNo).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.CloseType, x.LedgerPeriodId, x.FiscalYearId, x.RunMode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.CloseType, x.RunStatus });
        builder.HasIndex(x => new { x.LedgerPeriodId, x.RunStatus });

        builder.Property(x => x.CloseRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CloseRunNo).HasMaxLength(50);
        builder.Property(x => x.CloseType).HasMaxLength(20);
        builder.Property(x => x.ClosePeriodName).HasMaxLength(30);
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.RunMode).HasMaxLength(30);
        builder.Property(x => x.StartedBy).HasMaxLength(100);
        builder.Property(x => x.CompletedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.ReopenReason).HasMaxLength(1000);
        builder.Property(x => x.TotalTasks).HasPrecision(28, 2);

        builder.HasOne<GlCloseProfile>().WithMany().HasForeignKey(x => x.CloseProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
