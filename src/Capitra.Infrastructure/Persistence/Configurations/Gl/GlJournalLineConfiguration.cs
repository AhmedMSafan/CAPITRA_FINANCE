using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalLineConfiguration : IEntityTypeConfiguration<GlJournalLine>
{
    public void Configure(EntityTypeBuilder<GlJournalLine> builder)
    {
        builder.ToTable("GL_JOURNAL_LINES");
        builder.HasKey(x => x.JournalLineId);
        builder.HasIndex(x => new { x.JournalHeaderId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.AccountCombinationId });
        builder.HasIndex(x => new { x.AccountCombinationId, x.CurrencyCode });

        builder.Property(x => x.JournalLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredDr).HasPrecision(22, 5);
        builder.Property(x => x.EnteredCr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.ConversionRate).HasPrecision(9, 6);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.ReferenceNo).HasMaxLength(120);
        builder.Property(x => x.SourceLineId).HasMaxLength(120);
        builder.Property(x => x.SourceLineNo).HasMaxLength(120);
        builder.Property(x => x.StatisticalAmount).HasPrecision(18, 2);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
