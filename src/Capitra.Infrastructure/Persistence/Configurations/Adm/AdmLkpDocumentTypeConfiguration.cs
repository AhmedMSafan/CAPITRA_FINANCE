using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDocumentTypeConfiguration : IEntityTypeConfiguration<AdmLkpDocumentType>
{
    public void Configure(EntityTypeBuilder<AdmLkpDocumentType> builder)
    {
        builder.ToTable("ADM_LKP_DOCUMENT_TYPES");
        builder.HasKey(x => x.DocumentTypeId);
        builder.HasIndex(x => x.DocumentTypeCode).IsUnique();

        builder.Property(x => x.DocumentTypeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(30);
        builder.Property(x => x.DocumentTypeName).HasMaxLength(100);
        builder.Property(x => x.DocumentTypeNameAr).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AllowedExtensions).HasMaxLength(200);
        builder.Property(x => x.GroupCode).HasMaxLength(50);
        builder.Property(x => x.DocumentCategory).HasMaxLength(50);

        builder.ConfigureAudit();
    }
}
