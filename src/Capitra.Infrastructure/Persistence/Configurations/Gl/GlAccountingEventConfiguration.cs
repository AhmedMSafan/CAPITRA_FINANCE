using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingEventConfiguration : IEntityTypeConfiguration<GlAccountingEvent>
{
    public void Configure(EntityTypeBuilder<GlAccountingEvent> builder)
    {
        builder.ToTable("GL_ACCOUNTING_EVENTS");
        builder.HasKey(x => x.AccountingEventId);
        builder.HasIndex(x => new { x.SourceId, x.SourceDocumentType, x.SourceDocumentId, x.EventTypeId }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.EventNo }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.AccountingDate, x.AccountingStatus });
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentType, x.SourceDocumentId });

        builder.Property(x => x.AccountingEventId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventNo).HasMaxLength(80);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EventAmount).HasPrecision(22, 5);
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(60);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(120);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(120);
        builder.Property(x => x.SourceTransactionId).HasMaxLength(120);
        builder.Property(x => x.EventStatus).HasMaxLength(30);
        builder.Property(x => x.AccountingStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingRuleSet>().WithMany().HasForeignKey(x => x.SelectedRuleSetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEventType>().WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
