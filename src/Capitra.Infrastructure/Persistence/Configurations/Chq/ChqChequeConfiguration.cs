using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqChequeConfiguration : IEntityTypeConfiguration<ChqCheque>
{
    public void Configure(EntityTypeBuilder<ChqCheque> builder)
    {
        builder.ToTable("CHQ_CHEQUES");
        builder.HasKey(x => x.ChequeId);
        builder.HasIndex(x => new { x.ChequeNo, x.IssuerBankCode }).IsUnique();
        builder.HasIndex(x => x.ChequeRef).IsUnique();
        builder.HasIndex(x => x.DepositBatchId);
        builder.HasIndex(x => x.CurrentLocationCode);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.DrawdownId);
        builder.HasIndex(x => x.DepositBankAccountId);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => new { x.IssuerBankCode, x.ChequeNo });
        builder.HasIndex(x => x.LegalEntityId);
        builder.HasIndex(x => x.ScheduleId);
        builder.HasIndex(x => new { x.SourceModule, x.SourceRefId });
        builder.HasIndex(x => x.StatusCode);

        builder.Property(x => x.ChequeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequeRef).HasMaxLength(50);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.IssuerBankName).HasMaxLength(300);
        builder.Property(x => x.IssuerBankCode).HasMaxLength(30);
        builder.Property(x => x.IssuerBankBranch).HasMaxLength(200);
        builder.Property(x => x.IssuerAccountNo).HasMaxLength(100);
        builder.Property(x => x.DrawerNameEn).HasMaxLength(300);
        builder.Property(x => x.DrawerNameAr).HasMaxLength(300);
        builder.Property(x => x.DrawerNationalId).HasMaxLength(50);
        builder.Property(x => x.CustomerId).HasMaxLength(100);
        builder.Property(x => x.CustomerNameEn).HasMaxLength(300);
        builder.Property(x => x.CustomerNameAr).HasMaxLength(300);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceRefType).HasMaxLength(50);
        builder.Property(x => x.SourceRefId).HasMaxLength(100);
        builder.Property(x => x.StatusCode).HasMaxLength(50);
        builder.Property(x => x.RejectionReasonCode).HasMaxLength(50);
        builder.Property(x => x.RejectionNotes).HasMaxLength(2000);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.DrawerAccountNo).HasMaxLength(100);
        builder.Property(x => x.PartyTaxId).HasMaxLength(20);
        builder.Property(x => x.CurrentLocationCode).HasMaxLength(50);
        builder.Property(x => x.CurrentCustodianUser).HasMaxLength(100);

        builder.HasOne<ChqLkpLocation>().WithMany().HasForeignKey(x => x.CurrentLocationCode).OnDelete(DeleteBehavior.Restrict);
        // DRAWDOWN_ID -> FND_DRAWDOWN: added when the FND tables are ported.
        builder.HasOne<ChqLkpStatus>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        // SCHEDULE_ID -> FND_REPAYMENT_SCHEDULE: added when the FND tables are ported.
        // DEPOSIT_BANK_ACCOUNT_ID -> FND_BANK_ACCOUNT: added when the FND tables are ported.
        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        // PAYMENT_ID -> FND_REPAYMENT_PAYMENT: added when the FND tables are ported.
        builder.HasOne<ChqLkpRejectionReason>().WithMany().HasForeignKey(x => x.RejectionReasonCode).OnDelete(DeleteBehavior.Restrict);
        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        builder.ConfigureAudit();
    }
}
