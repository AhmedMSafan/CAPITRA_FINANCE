using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountBalanceConfiguration : IEntityTypeConfiguration<GlAccountBalance>
{
    public void Configure(EntityTypeBuilder<GlAccountBalance> builder)
    {
        builder.ToTable("GL_ACCOUNT_BALANCES");
        builder.HasKey(x => x.BalanceId);
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.AccountCombinationId, x.CurrencyCode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.CurrencyCode });
        builder.HasIndex(x => x.AccountCombinationId);

        builder.Property(x => x.BalanceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.OpeningDr).HasPrecision(22, 5);
        builder.Property(x => x.OpeningCr).HasPrecision(22, 5);
        builder.Property(x => x.PeriodDr).HasPrecision(22, 5);
        builder.Property(x => x.PeriodCr).HasPrecision(22, 5);
        builder.Property(x => x.ClosingDr).HasPrecision(22, 5);
        builder.Property(x => x.ClosingCr).HasPrecision(22, 5);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.LastPostedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
