using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqReplacementConfiguration : IEntityTypeConfiguration<ChqReplacement>
{
    public void Configure(EntityTypeBuilder<ChqReplacement> builder)
    {
        builder.ToTable("CHQ_REPLACEMENTS");
        builder.HasKey(x => x.ReplacementId);
        builder.HasIndex(x => x.ReplacementRef).IsUnique();
        builder.HasIndex(x => x.SettledAt);
        builder.HasIndex(x => x.SettlementMethod);
        builder.HasIndex(x => x.NewChequeId);
        builder.HasIndex(x => x.OriginalChequeId);
        builder.HasIndex(x => x.ReplacementStatus);

        builder.Property(x => x.ReplacementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReplacementRef).HasMaxLength(50);
        builder.Property(x => x.TriggerEvent).HasMaxLength(20);
        builder.Property(x => x.SettlementMethod).HasMaxLength(30);
        builder.Property(x => x.SettlementAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.OriginalChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.TransferTxnRef).HasMaxLength(200);
        builder.Property(x => x.InternalFundSource).HasMaxLength(200);
        builder.Property(x => x.InternalApprovalBy).HasMaxLength(100);
        builder.Property(x => x.ReplacementStatus).HasMaxLength(20);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        // TRANSFER_BANK_ACCOUNT_ID -> FND_BANK_ACCOUNT: added when the FND tables are ported.
        // PAYMENT_ID -> FND_REPAYMENT_PAYMENT: added when the FND tables are ported.
        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        builder.ConfigureAudit();
    }
}
