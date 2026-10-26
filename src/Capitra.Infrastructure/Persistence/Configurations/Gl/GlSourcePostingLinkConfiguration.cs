using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlSourcePostingLinkConfiguration : IEntityTypeConfiguration<GlSourcePostingLink>
{
    public void Configure(EntityTypeBuilder<GlSourcePostingLink> builder)
    {
        builder.ToTable("GL_SOURCE_POSTING_LINKS");
        builder.HasKey(x => x.SourcePostingLinkId);
        builder.HasIndex(x => new { x.SourceId, x.SourceDocumentType, x.SourceDocumentId, x.JournalHeaderId, x.JournalLineId }).IsUnique();
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentType, x.SourceDocumentId });
        builder.HasIndex(x => new { x.JournalHeaderId, x.JournalLineId });

        builder.Property(x => x.SourcePostingLinkId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(60);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(120);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(120);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);

        builder.HasOne<GlAccountingEvent>().WithMany().HasForeignKey(x => x.AccountingEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourcePostingLink>().WithMany().HasForeignKey(x => x.ReversalLinkId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
