using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffOrganizationConfiguration : IEntityTypeConfiguration<MffOrganization>
{
    public void Configure(EntityTypeBuilder<MffOrganization> builder)
    {
        builder.ToTable("MFF_ORGANIZATIONS");
        builder.HasKey(x => x.OrgId);
        builder.HasIndex(x => x.OrgCode).IsUnique();
        builder.HasIndex(x => x.OrgTypeCode);
        builder.HasIndex(x => x.RegulatorCode);

        builder.Property(x => x.OrgId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OrgCode).HasMaxLength(50);
        builder.Property(x => x.OrgNameAr).HasMaxLength(250);
        builder.Property(x => x.LegacyNameAr).HasMaxLength(250);
        builder.Property(x => x.OrgNameEn).HasMaxLength(250);
        builder.Property(x => x.OrgTypeCode).HasMaxLength(50);
        builder.Property(x => x.RegulatorCode).HasMaxLength(50);
        builder.Property(x => x.LicenseNo).HasMaxLength(100);
        builder.Property(x => x.BankCode).HasMaxLength(50);
        builder.Property(x => x.TaxId).HasMaxLength(50);
        builder.Property(x => x.AddressAr).HasMaxLength(500);
        builder.Property(x => x.PhoneNo).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);

        builder.HasOne<MffLkpOrgType>().WithMany().HasForeignKey(x => x.OrgTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpRegulator>().WithMany().HasForeignKey(x => x.RegulatorCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
