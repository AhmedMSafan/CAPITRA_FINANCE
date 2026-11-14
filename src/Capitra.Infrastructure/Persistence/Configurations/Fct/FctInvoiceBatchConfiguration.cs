using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctInvoiceBatchConfiguration : IEntityTypeConfiguration<FctInvoiceBatch>
{
    public void Configure(EntityTypeBuilder<FctInvoiceBatch> builder)
    {
        builder.ToTable("FCT_INVOICE_BATCHES");
        builder.HasKey(x => x.BatchId);
        builder.HasIndex(x => x.DisbursementId);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.BatchDate);
        builder.HasIndex(x => x.SubmissionMethod);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.SubmissionAt);
        builder.HasIndex(x => x.BatchType);
        builder.HasIndex(x => x.BatchNumber).IsUnique();

        builder.Property(x => x.BatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchNumber).HasMaxLength(30);
        builder.Property(x => x.BatchRef).HasMaxLength(50);
        builder.Property(x => x.BatchName).HasMaxLength(500);
        builder.Property(x => x.BatchNameAr).HasMaxLength(500);
        builder.Property(x => x.SubmissionMethod).HasMaxLength(30);
        builder.Property(x => x.SubmissionMethodAr).HasMaxLength(30);
        builder.Property(x => x.BatchType).HasMaxLength(30);
        builder.Property(x => x.BatchTypeAr).HasMaxLength(30);
        builder.Property(x => x.TotalGrossAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalNetAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalTaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        builder.Property(x => x.RejectedAmount).HasPrecision(18, 2);
        builder.Property(x => x.UploadFileName).HasMaxLength(500);
        builder.Property(x => x.UploadFileType).HasMaxLength(100);
        builder.Property(x => x.UploadTemplateVer).HasMaxLength(20);
        builder.Property(x => x.SubmittedBy).HasMaxLength(200);
        builder.Property(x => x.ValidatedBy).HasMaxLength(200);
        builder.Property(x => x.ApprovedBy).HasMaxLength(200);
        builder.Property(x => x.Remarks).HasMaxLength(2000);
        builder.Property(x => x.RemarksAr).HasMaxLength(2000);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
