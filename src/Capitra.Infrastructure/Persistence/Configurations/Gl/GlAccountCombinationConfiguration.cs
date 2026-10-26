using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountCombinationConfiguration : IEntityTypeConfiguration<GlAccountCombination>
{
    public void Configure(EntityTypeBuilder<GlAccountCombination> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATIONS");
        builder.HasKey(x => x.AccountCombinationId);
        builder.HasIndex(x => new { x.CoaId, x.ConcatenatedAccount }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.IsEnabled, x.ValidationStatus });
        builder.HasIndex(x => new { x.LegalEntityId, x.BranchId, x.CostCenterId });
        builder.HasIndex(x => new { x.BusinessLineCode, x.AccountId });
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.CompanySegmentValueId, x.AccountSegmentValueId });

        builder.Property(x => x.AccountCombinationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ConcatenatedAccount).HasMaxLength(400);
        builder.Property(x => x.DisplayAccount).HasMaxLength(500);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationMessage).HasMaxLength(1000);

        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCostCenter>().WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.CompanySegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.BranchSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.CostCenterSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.BusinessLineSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.AccountSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
