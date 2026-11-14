using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlYearEndCloseLineConfiguration : IEntityTypeConfiguration<GlYearEndCloseLine>
{
    public void Configure(EntityTypeBuilder<GlYearEndCloseLine> builder)
    {
        builder.ToTable("GL_YEAR_END_CLOSE_LINES");
        builder.HasKey(x => x.YearEndCloseLineId);
        builder.HasIndex(x => new { x.YearEndRunId, x.AccountCombinationId }).IsUnique();
        builder.HasIndex(x => new { x.YearEndRunId, x.ClosingBehavior, x.LineStatus });

        builder.Property(x => x.YearEndCloseLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountType).HasMaxLength(30);
        builder.Property(x => x.ClosingBehavior).HasMaxLength(30);
        builder.Property(x => x.BeginningBalanceDr).HasPrecision(28, 2);
        builder.Property(x => x.BeginningBalanceCr).HasPrecision(28, 2);
        builder.Property(x => x.PeriodDebit).HasPrecision(28, 2);
        builder.Property(x => x.PeriodCredit).HasPrecision(28, 2);
        builder.Property(x => x.EndingBalanceDr).HasPrecision(28, 2);
        builder.Property(x => x.EndingBalanceCr).HasPrecision(28, 2);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlYearEndCloseRun>().WithMany().HasForeignKey(x => x.YearEndRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseAccountGroup>().WithMany().HasForeignKey(x => x.AccountGroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.ClosingJournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.OpeningJournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
