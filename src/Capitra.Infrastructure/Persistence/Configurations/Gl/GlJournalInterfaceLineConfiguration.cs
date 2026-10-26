using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalInterfaceLineConfiguration : IEntityTypeConfiguration<GlJournalInterfaceLine>
{
    public void Configure(EntityTypeBuilder<GlJournalInterfaceLine> builder)
    {
        builder.ToTable("GL_JOURNAL_INTERFACE_LINES");
        builder.HasKey(x => x.InterfaceLineId);
        builder.HasIndex(x => new { x.InterfaceBatchId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.InterfaceBatchId, x.ValidationStatus });
        builder.HasIndex(x => new { x.SourceModule, x.SourceDocumentType, x.SourceDocumentId });

        builder.Property(x => x.InterfaceLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceLineRef).HasMaxLength(120);
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(60);
        builder.Property(x => x.SourceDocumentId).HasMaxLength(120);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(120);
        builder.Property(x => x.CategoryCode).HasMaxLength(30);
        builder.Property(x => x.CombinationCode).HasMaxLength(240);
        builder.Property(x => x.EnteredCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredDr).HasPrecision(22, 5);
        builder.Property(x => x.EnteredCr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.AccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.AccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.RateTypeCode).HasMaxLength(30);
        builder.Property(x => x.ConversionRate).HasPrecision(9, 6);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationErrorCode).HasMaxLength(60);
        builder.Property(x => x.ValidationErrorMsg).HasMaxLength(4000);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalInterfaceBatch>().WithMany().HasForeignKey(x => x.InterfaceBatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlAccountingEvent>().WithMany().HasForeignKey(x => x.AccountingEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.GeneratedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.GeneratedLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
