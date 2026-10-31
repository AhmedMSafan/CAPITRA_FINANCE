using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqUploadBatchConfiguration : IEntityTypeConfiguration<ChqUploadBatch>
{
    public void Configure(EntityTypeBuilder<ChqUploadBatch> builder)
    {
        builder.ToTable("CHQ_UPLOAD_BATCHES");
        builder.HasKey(x => x.BatchId);
        builder.HasIndex(x => x.BatchNo).IsUnique();

        builder.Property(x => x.BatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchNo).HasMaxLength(50);
        builder.Property(x => x.BatchStatus).HasMaxLength(30);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.MimeType).HasMaxLength(300);
        builder.Property(x => x.CustomerId).HasMaxLength(100);
        builder.Property(x => x.CustomerNameEn).HasMaxLength(300);
        builder.Property(x => x.CustomerNameAr).HasMaxLength(300);
        builder.Property(x => x.DrawerNameEn).HasMaxLength(300);
        builder.Property(x => x.DrawerNameAr).HasMaxLength(300);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceRefType).HasMaxLength(50);
        builder.Property(x => x.SourceRefId).HasMaxLength(100);
        builder.Property(x => x.DefaultLocationCode).HasMaxLength(50);
        builder.Property(x => x.DefaultCustodianUser).HasMaxLength(100);
        builder.Property(x => x.TotalRows).HasPrecision(28, 2);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.SubmittedBy).HasMaxLength(100);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);

        builder.ConfigureAudit();
    }
}
