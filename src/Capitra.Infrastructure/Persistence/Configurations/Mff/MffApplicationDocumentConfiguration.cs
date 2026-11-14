using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffApplicationDocumentConfiguration : IEntityTypeConfiguration<MffApplicationDocument>
{
    public void Configure(EntityTypeBuilder<MffApplicationDocument> builder)
    {
        builder.ToTable("MFF_APPLICATION_DOCUMENTS");
        builder.HasKey(x => x.DocumentId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.DocumentTypeCode);
        builder.HasIndex(x => x.PersonId);

        builder.Property(x => x.DocumentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(50);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.DocumentNo).HasMaxLength(200);
        builder.Property(x => x.VerificationStatus).HasMaxLength(30);
        builder.Property(x => x.VerifiedBy).HasMaxLength(100);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);

        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDocumentType>().WithMany().HasForeignKey(x => x.DocumentTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
