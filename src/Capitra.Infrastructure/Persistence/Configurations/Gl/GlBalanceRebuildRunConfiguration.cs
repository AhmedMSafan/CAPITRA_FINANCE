using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlBalanceRebuildRunConfiguration : IEntityTypeConfiguration<GlBalanceRebuildRun>
{
    public void Configure(EntityTypeBuilder<GlBalanceRebuildRun> builder)
    {
        builder.ToTable("GL_BALANCE_REBUILD_RUNS");
        builder.HasKey(x => x.BalanceRebuildRunId);
        builder.HasIndex(x => x.RebuildRunNo).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.RebuildStatus });
        builder.HasIndex(x => new { x.AccountCombinationId, x.CurrencyCode });

        builder.Property(x => x.BalanceRebuildRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RebuildRunNo).HasMaxLength(50);
        builder.Property(x => x.RebuildScope).HasMaxLength(30);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RebuildReason).HasMaxLength(1000);
        builder.Property(x => x.RebuildStatus).HasMaxLength(30);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
