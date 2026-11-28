using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOpeningBalanceLineConfiguration : IEntityTypeConfiguration<GlOpeningBalanceLine>
{
    public void Configure(EntityTypeBuilder<GlOpeningBalanceLine> builder)
    {
        builder.ToTable("GL_OPENING_BALANCE_LINES");
        builder.HasKey(x => x.OpeningBalanceLineId);
        builder.HasIndex(x => new { x.OpeningBalanceRunId, x.AccountCombinationId }).IsUnique();
        builder.HasIndex(x => new { x.OpeningBalanceRunId, x.LineStatus });

        builder.Property(x => x.OpeningBalanceLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OpeningDrAmount).HasPrecision(18, 2);
        builder.Property(x => x.OpeningCrAmount).HasPrecision(18, 2);
        builder.Property(x => x.SourceEndingDrAmount).HasPrecision(18, 2);
        builder.Property(x => x.SourceEndingCrAmount).HasPrecision(18, 2);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlOpeningBalanceRun>().WithMany().HasForeignKey(x => x.OpeningBalanceRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
