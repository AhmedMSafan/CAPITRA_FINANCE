using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffSubsidyMovementConfiguration : IEntityTypeConfiguration<MffSubsidyMovement>
{
    public void Configure(EntityTypeBuilder<MffSubsidyMovement> builder)
    {
        builder.ToTable("MFF_SUBSIDY_MOVEMENTS");
        builder.HasKey(x => x.MovementId);
        builder.HasIndex(x => x.InstallmentId);
        builder.HasIndex(x => x.SubsidyAccountId);

        builder.Property(x => x.MovementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.MovementType).HasMaxLength(30);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.DescriptionAr).HasMaxLength(1000);

        builder.HasOne<MffSubsidyAccount>().WithMany().HasForeignKey(x => x.SubsidyAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffInstallment>().WithMany().HasForeignKey(x => x.InstallmentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
