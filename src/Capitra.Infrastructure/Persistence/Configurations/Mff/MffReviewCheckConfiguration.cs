using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffReviewCheckConfiguration : IEntityTypeConfiguration<MffReviewCheck>
{
    public void Configure(EntityTypeBuilder<MffReviewCheck> builder)
    {
        builder.ToTable("MFF_REVIEW_CHECKS");
        builder.HasKey(x => x.CheckId);
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.CheckId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CheckType).HasMaxLength(50);
        builder.Property(x => x.CheckCode).HasMaxLength(100);
        builder.Property(x => x.ResultCode).HasMaxLength(30);
        builder.Property(x => x.CheckedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
