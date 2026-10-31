using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseRecurringRunConfiguration : IEntityTypeConfiguration<GlCloseRecurringRun>
{
    public void Configure(EntityTypeBuilder<GlCloseRecurringRun> builder)
    {
        builder.ToTable("GL_CLOSE_RECURRING_RUNS");
        builder.HasKey(x => x.CloseRecurringRunId);

        builder.Property(x => x.CloseRecurringRunId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunStatus).HasMaxLength(30);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRecurringJournalTemplate>().WithMany().HasForeignKey(x => x.RecurringTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
