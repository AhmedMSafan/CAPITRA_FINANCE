using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRecurringJournalRunConfiguration : IEntityTypeConfiguration<GlRecurringJournalRun>
{
    public void Configure(EntityTypeBuilder<GlRecurringJournalRun> builder)
    {
        builder.ToTable("GL_RECURRING_JOURNAL_RUNS");
        builder.HasKey(x => x.RecurringRunId);
        builder.HasIndex(x => new { x.RecurringTemplateId, x.LedgerPeriodId }).IsUnique();

        builder.Property(x => x.RecurringRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.GeneratedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRecurringJournalTemplate>().WithMany().HasForeignKey(x => x.RecurringTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
