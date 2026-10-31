using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpDocumentTypeConfiguration : IEntityTypeConfiguration<MffLkpDocumentType>
{
    public void Configure(EntityTypeBuilder<MffLkpDocumentType> builder)
    {
        builder.ToTable("MFF_LKP_DOCUMENT_TYPES");
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
