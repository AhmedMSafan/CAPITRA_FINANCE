using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingRollbackLineConfiguration : IEntityTypeConfiguration<GlPostingRollbackLine>
{
    public void Configure(EntityTypeBuilder<GlPostingRollbackLine> builder)
    {
        builder.ToTable("GL_POSTING_ROLLBACK_LINES");
        builder.HasKey(x => x.RollbackLineId);
        builder.HasIndex(x => new { x.RollbackRunId, x.RollbackStatus });
        builder.HasIndex(x => new { x.JournalHeaderId, x.JournalLineId });

        builder.Property(x => x.RollbackLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RollbackAccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.RollbackAccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.RollbackStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlPostingRollbackRun>().WithMany().HasForeignKey(x => x.RollbackRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingBalanceImpact>().WithMany().HasForeignKey(x => x.BalanceImpactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
