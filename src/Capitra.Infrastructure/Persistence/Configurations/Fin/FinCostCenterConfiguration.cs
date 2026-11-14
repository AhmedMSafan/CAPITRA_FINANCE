using Capitra.Domain.Modules.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinCostCenterConfiguration : IEntityTypeConfiguration<FinCostCenter>
{
    public void Configure(EntityTypeBuilder<FinCostCenter> builder)
    {
        builder.ToTable("FIN_COST_CENTERS");
        builder.HasKey(x => x.CostCenterId);
        builder.HasIndex(x => new { x.LegalEntityId, x.CostCenterCode }).IsUnique();
        builder.HasIndex(x => new { x.LegalEntityId, x.BranchId, x.IsActive });
        builder.HasIndex(x => x.ParentCostCenterId);

        builder.Property(x => x.CostCenterId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CostCenterCode).HasMaxLength(30);
        builder.Property(x => x.CostCenterNameEn).HasMaxLength(150);
        builder.Property(x => x.CostCenterNameAr).HasMaxLength(150);
        builder.Property(x => x.CostCenterType).HasMaxLength(30);
        builder.Property(x => x.ManagerUserName).HasMaxLength(100);

        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCostCenter>().WithMany().HasForeignKey(x => x.ParentCostCenterId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
