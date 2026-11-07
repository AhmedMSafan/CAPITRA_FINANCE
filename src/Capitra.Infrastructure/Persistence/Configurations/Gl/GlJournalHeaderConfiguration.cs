using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalHeaderConfiguration : IEntityTypeConfiguration<GlJournalHeader>
{
    public void Configure(EntityTypeBuilder<GlJournalHeader> builder)
    {
        builder.ToTable("GL_JOURNAL_HEADERS");
        builder.HasKey(x => x.JournalHeaderId);
        builder.HasIndex(x => new { x.LedgerId, x.JournalNo }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.JournalStatus });
        builder.HasIndex(x => new { x.SourceId, x.SourceDocumentType, x.SourceDocumentId });
        builder.HasIndex(x => x.AccountingDate);
        builder.HasIndex(x => new { x.PostingStatus, x.ApprovalStatus });

        builder.Property(x => x.JournalHeaderId).UseIdentityByDefaultColumn();
        builder.Property(x => x.JournalNo).HasMaxLength(60);
        builder.Property(x => x.JournalName).HasMaxLength(240);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ConversionRate).HasPrecision(9, 6);
        builder.Property(x => x.JournalStatus).HasMaxLength(30);
        builder.Property(x => x.ApprovalStatus).HasMaxLength(30);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.ReversalMethod).HasMaxLength(30);
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(60);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(120);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ReferenceNo).HasMaxLength(120);
        builder.Property(x => x.SubmittedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);
        builder.Property(x => x.PostedBy).HasMaxLength(255);
        builder.Property(x => x.CancelledBy).HasMaxLength(255);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.OriginalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.ReversedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.ReversalPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
