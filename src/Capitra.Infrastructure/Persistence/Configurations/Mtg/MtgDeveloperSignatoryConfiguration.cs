using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgDeveloperSignatoryConfiguration : IEntityTypeConfiguration<MtgDeveloperSignatory>
{
    public void Configure(EntityTypeBuilder<MtgDeveloperSignatory> builder)
    {
        builder.ToTable("MTG_DEVELOPER_SIGNATORIES");
        builder.HasKey(x => x.SignatoryId);
        builder.HasIndex(x => x.DeveloperId);

        builder.Property(x => x.SignatoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SignatoryName).HasMaxLength(300);
        builder.Property(x => x.NationalId).HasMaxLength(50);
        builder.Property(x => x.JobTitle).HasMaxLength(200);
        builder.Property(x => x.AuthorizationReference).HasMaxLength(300);

        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
