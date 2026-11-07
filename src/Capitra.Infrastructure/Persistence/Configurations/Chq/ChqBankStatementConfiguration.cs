using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqBankStatementConfiguration : IEntityTypeConfiguration<ChqBankStatement>
{
    public void Configure(EntityTypeBuilder<ChqBankStatement> builder)
    {
        builder.ToTable("CHQ_BANK_STATEMENTS");
        builder.HasKey(x => x.StatementLineId);
        builder.HasIndex(x => new { x.StatementRef, x.BankAccountId }).IsUnique();
        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.MatchedChequeId);
        builder.HasIndex(x => x.LegalEntityId);
        builder.HasIndex(x => x.ReconStatus);

        builder.Property(x => x.StatementLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatementRef).HasMaxLength(50);
        builder.Property(x => x.TransactionType).HasMaxLength(20);
        builder.Property(x => x.TransactionAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.BankReference).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ReconStatus).HasMaxLength(20);
        builder.Property(x => x.ReconBy).HasMaxLength(100);
        builder.Property(x => x.ReconNotes).HasMaxLength(1000);
        builder.Property(x => x.ClosingBalance).HasPrecision(20, 4);

        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        // BANK_ACCOUNT_ID -> FND_BANK_ACCOUNT: added when the FND tables are ported.
        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        builder.ConfigureAudit();
    }
}
