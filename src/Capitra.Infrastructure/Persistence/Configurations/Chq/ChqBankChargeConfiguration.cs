using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqBankChargeConfiguration : IEntityTypeConfiguration<ChqBankCharge>
{
    public void Configure(EntityTypeBuilder<ChqBankCharge> builder)
    {
        builder.ToTable("CHQ_BANK_CHARGES");
        builder.HasKey(x => x.ChargeId);
        builder.HasIndex(x => x.ChargeRef).IsUnique();
        builder.HasIndex(x => x.ChequeId);
        builder.HasIndex(x => x.ChargeAt);
        builder.HasIndex(x => x.IsGlPosted);
        builder.HasIndex(x => x.LegalEntityId);

        builder.Property(x => x.ChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChargeRef).HasMaxLength(50);
        builder.Property(x => x.ChargeType).HasMaxLength(50);
        builder.Property(x => x.ChargeDescriptionEn).HasMaxLength(300);
        builder.Property(x => x.ChargeDescriptionAr).HasMaxLength(300);
        builder.Property(x => x.ChargeAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ChargedByBank).HasMaxLength(300);
        builder.Property(x => x.RecoveryAmount).HasPrecision(18, 2);
        builder.Property(x => x.GlDebitAccount).HasMaxLength(50);
        builder.Property(x => x.GlCreditAccount).HasMaxLength(50);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        // GL_CREDIT_ACCOUNT -> LKP_GL_ACCOUNT: added when the LKP tables are ported.
        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        // GL_DEBIT_ACCOUNT -> LKP_GL_ACCOUNT: added when the LKP tables are ported.
        builder.ConfigureAudit();
    }
}
