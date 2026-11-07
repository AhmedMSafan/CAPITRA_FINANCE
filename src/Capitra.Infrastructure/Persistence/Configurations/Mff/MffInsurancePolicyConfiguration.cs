using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffInsurancePolicyConfiguration : IEntityTypeConfiguration<MffInsurancePolicy>
{
    public void Configure(EntityTypeBuilder<MffInsurancePolicy> builder)
    {
        builder.ToTable("MFF_INSURANCE_POLICIES");
        builder.HasKey(x => x.PolicyId);
        builder.HasIndex(x => x.PolicyNo).IsUnique();
        builder.HasIndex(x => x.LoanId);
        builder.HasIndex(x => x.InsurerOrgId);

        builder.Property(x => x.PolicyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PolicyNo).HasMaxLength(100);
        builder.Property(x => x.PolicyType).HasMaxLength(30);
        builder.Property(x => x.PremiumAmount).HasPrecision(18, 2);
        builder.Property(x => x.InsurancePolicyStatus).HasMaxLength(30);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.InsurerOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
