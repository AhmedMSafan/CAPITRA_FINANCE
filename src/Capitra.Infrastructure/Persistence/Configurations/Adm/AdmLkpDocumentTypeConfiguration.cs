using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDocumentTypeConfiguration : LookupConfiguration<AdmLkpDocumentType>
{
    protected override string TableName => "ADM_LKP_DOCUMENT_TYPE";

    protected override int NameMaxLength => 100;

    public override void Configure(EntityTypeBuilder<AdmLkpDocumentType> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.DocumentGroupCode).HasMaxLength(LookupCodeLength);
        builder.Property(x => x.DocumentCategory).HasMaxLength(50);
        builder.Property(x => x.AllowedExtensions).HasMaxLength(200);
        builder.HasOne<AdmLkpDocumentGroup>().WithMany().HasForeignKey(x => x.DocumentGroupCode).OnDelete(DeleteBehavior.Restrict);
    }
}
