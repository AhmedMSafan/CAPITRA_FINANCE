using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlLedgerOptionConfiguration : IEntityTypeConfiguration<GlLedgerOption>
{
    public void Configure(EntityTypeBuilder<GlLedgerOption> builder)
    {
        builder.ToTable("GL_LEDGER_OPTIONS");
        builder.HasKey(x => x.LedgerOptionId);
        builder.HasIndex(x => x.LedgerId).IsUnique();

        builder.Property(x => x.LedgerOptionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AllowManualJournals).HasMaxLength(1);
        builder.Property(x => x.RequireManualJournalAppr).HasMaxLength(1);
        builder.Property(x => x.AllowSubledgerJournals).HasMaxLength(1);
        builder.Property(x => x.AllowFuturePeriodEntry).HasMaxLength(1);
        builder.Property(x => x.AllowBackdatedEntry).HasMaxLength(1);
        builder.Property(x => x.AllowReversalJournals).HasMaxLength(1);
        builder.Property(x => x.RequireBalancedJournal).HasMaxLength(1);
        builder.Property(x => x.AllowStatisticalJournals).HasMaxLength(1);
        builder.Property(x => x.DefaultReversalMethod).HasMaxLength(30);
        builder.Property(x => x.PostingMode).HasMaxLength(30);
        builder.Property(x => x.PeriodClosePolicy).HasMaxLength(30);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
