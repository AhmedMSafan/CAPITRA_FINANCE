using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqEventConfiguration : IEntityTypeConfiguration<ChqEvent>
{
    public void Configure(EntityTypeBuilder<ChqEvent> builder)
    {
        builder.ToTable("CHQ_EVENTS");
        builder.HasKey(x => x.EventId);
        builder.HasIndex(x => x.DepositBatchId);
        builder.HasIndex(x => x.ChequeId);
        builder.HasIndex(x => x.IsGlPosted);
        builder.HasIndex(x => x.MovementGroupId);
        builder.HasIndex(x => x.ToLocationCode);
        builder.HasIndex(x => x.EventType);

        builder.Property(x => x.EventId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventType).HasMaxLength(30);
        builder.Property(x => x.EventAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RejectionReasonCode).HasMaxLength(50);
        builder.Property(x => x.BankRef).HasMaxLength(200);
        builder.Property(x => x.NewStatusCode).HasMaxLength(50);
        builder.Property(x => x.OldStatusCode).HasMaxLength(50);
        builder.Property(x => x.GlJournalRef).HasMaxLength(100);
        builder.Property(x => x.GlDebitAccount).HasMaxLength(50);
        builder.Property(x => x.GlCreditAccount).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.FromLocationCode).HasMaxLength(50);
        builder.Property(x => x.ToLocationCode).HasMaxLength(50);
        builder.Property(x => x.FromCustodianUser).HasMaxLength(100);
        builder.Property(x => x.ToCustodianUser).HasMaxLength(100);
        builder.Property(x => x.HandoverBy).HasMaxLength(100);
        builder.Property(x => x.ReceivedBy).HasMaxLength(100);
        builder.Property(x => x.MovementRef).HasMaxLength(200);
        builder.Property(x => x.SourceSessionId).HasMaxLength(100);

        // CURRENCY_CODE -> LKP_CURRENCY: added when the LKP tables are ported.
        builder.HasOne<ChqLkpStatus>().WithMany().HasForeignKey(x => x.OldStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ChqLkpStatus>().WithMany().HasForeignKey(x => x.NewStatusCode).OnDelete(DeleteBehavior.Restrict);
        // GL_DEBIT_ACCOUNT -> LKP_GL_ACCOUNT: added when the LKP tables are ported.
        builder.HasOne<ChqDepositBatch>().WithMany().HasForeignKey(x => x.DepositBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ChqLkpRejectionReason>().WithMany().HasForeignKey(x => x.RejectionReasonCode).OnDelete(DeleteBehavior.Restrict);
        // GL_CREDIT_ACCOUNT -> LKP_GL_ACCOUNT: added when the LKP tables are ported.
        builder.ConfigureAudit();
    }
}
