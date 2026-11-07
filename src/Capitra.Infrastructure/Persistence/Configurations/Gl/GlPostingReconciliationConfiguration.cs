using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingReconciliationConfiguration : IEntityTypeConfiguration<GlPostingReconciliation>
{
    public void Configure(EntityTypeBuilder<GlPostingReconciliation> builder)
    {
        builder.ToTable("GL_POSTING_RECONCILIATIONS");
        builder.HasKey(x => x.PostingReconId);
        builder.HasIndex(x => x.ReconNo).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.ReconStatus });
        builder.HasIndex(x => new { x.PostingRunId, x.ReconStatus });

        builder.Property(x => x.PostingReconId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReconNo).HasMaxLength(50);
        builder.Property(x => x.ReconScope).HasMaxLength(30);
        builder.Property(x => x.JournalTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.JournalTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.ImpactTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.ImpactTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.BalanceTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.DifferenceDr).HasPrecision(22, 5);
        builder.Property(x => x.DifferenceCr).HasPrecision(22, 5);
        builder.Property(x => x.ReconStatus).HasMaxLength(30);
        builder.Property(x => x.ReconciledBy).HasMaxLength(100);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
