using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlDailyBalanceConfiguration : IEntityTypeConfiguration<GlDailyBalance>
{
    public void Configure(EntityTypeBuilder<GlDailyBalance> builder)
    {
        builder.ToTable("GL_DAILY_BALANCES");
        builder.HasKey(x => x.DailyBalanceId);
        builder.HasIndex(x => new { x.LedgerId, x.BalanceDate, x.AccountCombinationId, x.CurrencyCode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.BalanceDate, x.CurrencyCode });

        builder.Property(x => x.DailyBalanceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.DailyDr).HasPrecision(22, 5);
        builder.Property(x => x.DailyCr).HasPrecision(22, 5);
        builder.Property(x => x.ClosingDr).HasPrecision(22, 5);
        builder.Property(x => x.ClosingCr).HasPrecision(22, 5);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
