using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctRepaymentTransactionConfiguration : IEntityTypeConfiguration<FctRepaymentTransaction>
{
    public void Configure(EntityTypeBuilder<FctRepaymentTransaction> builder)
    {
        builder.ToTable("FCT_REPAYMENT_TRANSACTIONS");
        builder.HasKey(x => x.RepaymentTxnId);
        builder.HasIndex(x => x.RepaymentNumber).IsUnique();
        builder.HasIndex(x => new { x.ClientId, x.LegalEntityId, x.ValueDate, x.StatusCode });

        builder.Property(x => x.RepaymentTxnId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RepaymentNumber).HasMaxLength(40);
        builder.Property(x => x.RepaymentMethod).HasMaxLength(30);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ReceiptAmount).HasPrecision(18, 2);
        builder.Property(x => x.AllocatedAmount).HasPrecision(18, 2);
        builder.Property(x => x.BankStatementReference).HasMaxLength(100);
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.ReversalReason).HasMaxLength(1000);
        builder.Property(x => x.ReversedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.ConfigureAudit();
    }
}
