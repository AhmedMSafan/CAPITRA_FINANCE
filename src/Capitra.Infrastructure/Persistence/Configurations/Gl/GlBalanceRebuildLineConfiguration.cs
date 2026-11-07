using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlBalanceRebuildLineConfiguration : IEntityTypeConfiguration<GlBalanceRebuildLine>
{
    public void Configure(EntityTypeBuilder<GlBalanceRebuildLine> builder)
    {
        builder.ToTable("GL_BALANCE_REBUILD_LINES");
        builder.HasKey(x => x.BalanceRebuildLineId);
        builder.HasIndex(x => new { x.BalanceRebuildRunId, x.RebuildStatus });
        builder.HasIndex(x => new { x.AccountCombinationId, x.LedgerPeriodId, x.CurrencyCode });

        builder.Property(x => x.BalanceRebuildLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.OldPeriodDr).HasPrecision(22, 5);
        builder.Property(x => x.OldPeriodCr).HasPrecision(22, 5);
        builder.Property(x => x.NewPeriodDr).HasPrecision(22, 5);
        builder.Property(x => x.NewPeriodCr).HasPrecision(22, 5);
        builder.Property(x => x.RebuildStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlBalanceRebuildRun>().WithMany().HasForeignKey(x => x.BalanceRebuildRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
