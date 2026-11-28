using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlLedgerPeriodConfiguration : IEntityTypeConfiguration<GlLedgerPeriod>
{
    public void Configure(EntityTypeBuilder<GlLedgerPeriod> builder)
    {
        builder.ToTable("GL_LEDGER_PERIODS");
        builder.HasKey(x => x.LedgerPeriodId);
        builder.HasIndex(x => new { x.LedgerId, x.PeriodId }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.PeriodStatus });
        builder.HasIndex(x => new { x.PeriodId, x.PeriodStatus });
        builder.HasIndex(x => new { x.LedgerId, x.OpenedAt, x.ClosedAt });

        builder.Property(x => x.LedgerPeriodId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PeriodStatus).HasMaxLength(30);
        builder.Property(x => x.PostingMode).HasMaxLength(30);
        builder.Property(x => x.OpenedBy).HasMaxLength(100);
        builder.Property(x => x.ClosedBy).HasMaxLength(100);
        builder.Property(x => x.PermanentlyClosedBy).HasMaxLength(100);
        builder.Property(x => x.CloseNotes).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingPeriod>().WithMany().HasForeignKey(x => x.PeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
