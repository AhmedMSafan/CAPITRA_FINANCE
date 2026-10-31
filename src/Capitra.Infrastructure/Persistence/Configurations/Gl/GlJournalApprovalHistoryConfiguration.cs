using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalApprovalHistoryConfiguration : IEntityTypeConfiguration<GlJournalApprovalHistory>
{
    public void Configure(EntityTypeBuilder<GlJournalApprovalHistory> builder)
    {
        builder.ToTable("GL_JOURNAL_APPROVAL_HIST");
        builder.HasKey(x => x.ApprovalHistoryId);

        builder.Property(x => x.ApprovalHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ApproverRoleCode).HasMaxLength(60);
        builder.Property(x => x.ApproverUser).HasMaxLength(255);
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ActionNotes).HasMaxLength(4000);
        builder.Property(x => x.DelegatedFromUser).HasMaxLength(255);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
