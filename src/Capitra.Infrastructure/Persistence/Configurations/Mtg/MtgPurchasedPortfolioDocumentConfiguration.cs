using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioDocumentConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioDocument>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioDocument> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_DOCUMENTS");
        builder.HasKey(x => x.DocumentId);
        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => x.DocumentTypeCode);

        builder.Property(x => x.DocumentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EntityType).HasMaxLength(30);
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(50);
        builder.Property(x => x.DocumentNo).HasMaxLength(100);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.MimeType).HasMaxLength(255);
        builder.Property(x => x.FileUrl).HasMaxLength(2000);
        builder.Property(x => x.VerifiedBy).HasMaxLength(255);
        builder.Property(x => x.DocumentStatus).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpDocumentType>().WithMany().HasPrincipalKey(x => x.DocumentTypeCode).HasForeignKey(x => x.DocumentTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
