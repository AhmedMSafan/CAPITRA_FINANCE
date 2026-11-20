using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingRunLineConfiguration : IEntityTypeConfiguration<GlPostingRunLine>
{
    public void Configure(EntityTypeBuilder<GlPostingRunLine> builder)
    {
        builder.ToTable("GL_POSTING_RUN_LINES");
        builder.HasKey(x => x.PostingRunLineId);
        builder.HasIndex(x => new { x.PostingRunId, x.JournalHeaderId }).IsUnique();
        builder.HasIndex(x => new { x.JournalHeaderId, x.PostingStatus });

        builder.Property(x => x.PostingRunLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorCode).HasMaxLength(60);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
