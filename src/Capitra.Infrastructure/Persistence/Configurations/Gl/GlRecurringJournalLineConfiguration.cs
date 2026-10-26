using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRecurringJournalLineConfiguration : IEntityTypeConfiguration<GlRecurringJournalLine>
{
    public void Configure(EntityTypeBuilder<GlRecurringJournalLine> builder)
    {
        builder.ToTable("GL_RECURRING_JOURNAL_LINES");
        builder.HasKey(x => x.RecurringLineId);
        builder.HasIndex(x => new { x.RecurringTemplateId, x.LineNo }).IsUnique();

        builder.Property(x => x.RecurringLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.EnteredDr).HasPrecision(22, 5);
        builder.Property(x => x.EnteredCr).HasPrecision(22, 5);
        builder.Property(x => x.AllocationFormula).HasMaxLength(1000);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRecurringJournalTemplate>().WithMany().HasForeignKey(x => x.RecurringTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
