using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctCollectionBatchConfiguration : IEntityTypeConfiguration<FctCollectionBatch>
{
    public void Configure(EntityTypeBuilder<FctCollectionBatch> builder)
    {
        builder.ToTable("FCT_COLLECTION_BATCHES");
        builder.HasKey(x => x.CollectionBatchId);
        builder.HasIndex(x => x.BatchNo).IsUnique();
        builder.HasIndex(x => x.GlTranSetId);

        builder.Property(x => x.CollectionBatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchNo).HasMaxLength(30);
        builder.Property(x => x.CollectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.BouncedAmount).HasPrecision(18, 2);
        builder.Property(x => x.RejectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.ConfigureAudit();
    }
}

public sealed class FctCollectionBatchLineConfiguration : IEntityTypeConfiguration<FctCollectionBatchLine>
{
    public void Configure(EntityTypeBuilder<FctCollectionBatchLine> builder)
    {
        builder.ToTable("FCT_COLLECTION_BATCH_LINES");
        builder.HasKey(x => x.CollectionBatchLineId);
        builder.HasIndex(x => new { x.CollectionBatchId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.ChequeId);
        builder.HasIndex(x => x.RepaymentTxnId);

        builder.Property(x => x.CollectionBatchLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequeRef).HasMaxLength(50);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.CustomerId).HasMaxLength(100);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ResultCode).HasMaxLength(20);
        builder.Property(x => x.BankStatementReference).HasMaxLength(100);
        builder.Property(x => x.ReasonCode).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne<FctCollectionBatch>().WithMany().HasForeignKey(x => x.CollectionBatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<FctRepaymentTransaction>().WithMany().HasForeignKey(x => x.RepaymentTxnId).OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
