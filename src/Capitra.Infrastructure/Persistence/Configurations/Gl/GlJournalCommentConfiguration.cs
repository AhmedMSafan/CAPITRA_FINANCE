using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalCommentConfiguration : IEntityTypeConfiguration<GlJournalComment>
{
    public void Configure(EntityTypeBuilder<GlJournalComment> builder)
    {
        builder.ToTable("GL_JOURNAL_COMMENTS");
        builder.HasKey(x => x.CommentId);

        builder.Property(x => x.CommentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CommentType).HasMaxLength(30);
        builder.Property(x => x.CommentText).HasMaxLength(4000);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
