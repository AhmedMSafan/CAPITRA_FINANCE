using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffAccountingEntryConfiguration : IEntityTypeConfiguration<MffAccountingEntry>
{
    public void Configure(EntityTypeBuilder<MffAccountingEntry> builder)
    {
        builder.ToTable("MFF_ACCOUNTING_ENTRIES");
        builder.HasKey(x => x.EntryId);
        builder.HasIndex(x => x.LoanId);
        builder.HasIndex(x => x.EventCode);
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.EntryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventCode).HasMaxLength(30);
        builder.Property(x => x.GlAccountCode).HasMaxLength(100);
        builder.Property(x => x.DebitAmount).HasPrecision(18, 2);
        builder.Property(x => x.CreditAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.CoreJournalRef).HasMaxLength(200);
        builder.Property(x => x.DescriptionAr).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpAccountEvent>().WithMany().HasForeignKey(x => x.EventCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
