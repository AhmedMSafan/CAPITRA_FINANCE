using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDocumentGroupConfiguration : IEntityTypeConfiguration<AdmLkpDocumentGroup>
{
    public void Configure(EntityTypeBuilder<AdmLkpDocumentGroup> builder)
    {
        builder.ToTable("ADM_LKP_DOCUMENT_GROUPS");
        builder.HasKey(x => x.GroupCode);

        builder.Property(x => x.GroupCode).HasMaxLength(30);
        builder.Property(x => x.GroupNameAr).HasMaxLength(200);
        builder.Property(x => x.GroupNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
