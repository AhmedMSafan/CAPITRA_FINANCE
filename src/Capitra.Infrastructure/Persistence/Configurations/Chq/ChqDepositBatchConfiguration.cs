using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqDepositBatchConfiguration : IEntityTypeConfiguration<ChqDepositBatch>
{
    public void Configure(EntityTypeBuilder<ChqDepositBatch> builder)
    {
        builder.ToTable("CHQ_DEPOSIT_BATCHES");
        builder.HasKey(x => x.DepositBatchId);
        builder.HasIndex(x => x.BatchRef).IsUnique();
        builder.HasIndex(x => x.DepositBankAccountId);
        builder.HasIndex(x => x.BatchDate);
        builder.HasIndex(x => x.LegalEntityId);
        builder.HasIndex(x => x.BatchStatus);

        builder.Property(x => x.DepositBatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchRef).HasMaxLength(50);
        builder.Property(x => x.TotalCheques).HasPrecision(28, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CollectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.RejectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.WithdrawnAmount).HasPrecision(18, 2);
        builder.Property(x => x.PendingAmount).HasPrecision(18, 2);
        builder.Property(x => x.BatchStatus).HasMaxLength(20);
        builder.Property(x => x.BankBatchRef).HasMaxLength(200);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        // DEPOSIT_BANK_ACCOUNT_ID -> FND_BANK_ACCOUNT: added when the FND tables are ported.
        builder.ConfigureAudit();
    }
}
