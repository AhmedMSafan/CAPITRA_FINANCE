using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctAgreementDocumentConfiguration : IEntityTypeConfiguration<FctAgreementDocument>
{
    public void Configure(EntityTypeBuilder<FctAgreementDocument> builder)
    {
        builder.ToTable("FCT_AGREEMENT_DOCUMENTS");
        builder.HasKey(x => x.DocumentId);
        builder.HasIndex(x => x.AgreementId);
        builder.HasIndex(x => x.ExpiryDate);
        builder.HasIndex(x => x.PreviousVersionId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.DocTypeId);

        builder.Property(x => x.DocumentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DocumentNumber).HasMaxLength(50);
        builder.Property(x => x.DocumentTitle).HasMaxLength(500);
        builder.Property(x => x.DocumentTitleAr).HasMaxLength(500);
        builder.Property(x => x.DocumentDesc).HasMaxLength(1000);
        builder.Property(x => x.DocumentDescAr).HasMaxLength(1000);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.FileExtension).HasMaxLength(10);
        builder.Property(x => x.MimeType).HasMaxLength(200);
        builder.Property(x => x.FilePath).HasMaxLength(1000);
        builder.Property(x => x.StorageType).HasMaxLength(20);
        builder.Property(x => x.NotarizationRef).HasMaxLength(100);
        builder.Property(x => x.VerifiedBy).HasMaxLength(100);
        builder.Property(x => x.Remarks).HasMaxLength(1000);
        builder.Property(x => x.RemarksAr).HasMaxLength(1000);

        builder.HasOne<FctAgreement>().WithMany().HasForeignKey(x => x.AgreementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpDocumentType>().WithMany().HasForeignKey(x => x.DocTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctAgreementDocument>().WithMany().HasForeignKey(x => x.PreviousVersionId).OnDelete(DeleteBehavior.Restrict);
        // as in FINOVA_FINANCEING: the document statuses, not the Oracle LKP_STATUS_CODES
        builder.HasOne<AdmLkpDocumentStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
