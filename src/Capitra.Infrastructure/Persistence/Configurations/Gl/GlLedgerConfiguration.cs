using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlLedgerConfiguration : IEntityTypeConfiguration<GlLedger>
{
    public void Configure(EntityTypeBuilder<GlLedger> builder)
    {
        builder.ToTable("GL_LEDGERS");
        builder.HasKey(x => x.LedgerId);
        builder.HasIndex(x => x.LedgerCode).IsUnique();
        builder.HasIndex(x => x.CoaId);
        builder.HasIndex(x => new { x.LegalEntityId, x.LedgerStatus });
        builder.HasIndex(x => x.CalendarId);

        builder.Property(x => x.LedgerId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LedgerCode).HasMaxLength(30);
        builder.Property(x => x.LedgerNameEn).HasMaxLength(150);
        builder.Property(x => x.LedgerNameAr).HasMaxLength(150);
        builder.Property(x => x.LedgerType).HasMaxLength(30);
        builder.Property(x => x.LedgerCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.LedgerStatus).HasMaxLength(30);
        builder.Property(x => x.RetainedEarningsAcctCode).HasMaxLength(100);
        builder.Property(x => x.SuspenseAcctCode).HasMaxLength(100);
        builder.Property(x => x.RoundingDiffAcctCode).HasMaxLength(100);
        builder.Property(x => x.FxGainAcctCode).HasMaxLength(100);
        builder.Property(x => x.FxLossAcctCode).HasMaxLength(100);
        builder.Property(x => x.OpeningBalanceAcctCode).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.LedgerCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
