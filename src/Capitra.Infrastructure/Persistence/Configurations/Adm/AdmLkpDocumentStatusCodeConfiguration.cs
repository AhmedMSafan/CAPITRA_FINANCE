using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDocumentStatusCodeConfiguration : IEntityTypeConfiguration<AdmLkpDocumentStatusCode>
{
    public void Configure(EntityTypeBuilder<AdmLkpDocumentStatusCode> builder)
    {
        builder.ToTable("ADM_LKP_DOCUMENT_STATUS_CODES");
        builder.HasKey(x => x.StatusId);
        builder.HasIndex(x => x.StatusCodeValue).IsUnique();

        builder.Property(x => x.StatusId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCodeValue).HasMaxLength(50);
        builder.Property(x => x.StatusName).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.ConfigureAudit();
    }
}
