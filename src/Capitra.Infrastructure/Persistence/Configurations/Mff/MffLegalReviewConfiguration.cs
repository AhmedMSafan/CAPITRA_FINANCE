using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLegalReviewConfiguration : IEntityTypeConfiguration<MffLegalReview>
{
    public void Configure(EntityTypeBuilder<MffLegalReview> builder)
    {
        builder.ToTable("MFF_LEGAL_REVIEWS");
        builder.HasKey(x => x.LegalReviewId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.PropertyId);

        builder.Property(x => x.LegalReviewId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReviewerUser).HasMaxLength(100);
        builder.Property(x => x.ResultCode).HasMaxLength(30);
        builder.Property(x => x.Issues).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProperty>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
