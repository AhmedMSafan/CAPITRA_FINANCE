using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalActionHistoryConfiguration : IEntityTypeConfiguration<GlJournalActionHistory>
{
    public void Configure(EntityTypeBuilder<GlJournalActionHistory> builder)
    {
        builder.ToTable("GL_JOURNAL_ACTION_HIST");
        builder.HasKey(x => x.ActionHistoryId);

        builder.Property(x => x.ActionHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(60);
        builder.Property(x => x.OldStatus).HasMaxLength(60);
        builder.Property(x => x.NewStatus).HasMaxLength(60);
        builder.Property(x => x.ActionReason).HasMaxLength(1000);
        builder.Property(x => x.ActionNotes).HasMaxLength(4000);
        builder.Property(x => x.ActionBy).HasMaxLength(255);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
