using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLegalReviewConfiguration : IEntityTypeConfiguration<MtgLegalReview>
{
    public void Configure(EntityTypeBuilder<MtgLegalReview> builder)
    {
        builder.ToTable("MTG_LEGAL_REVIEWS");
        builder.HasKey(x => x.LegalReviewId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.LegalReviewId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReviewedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
