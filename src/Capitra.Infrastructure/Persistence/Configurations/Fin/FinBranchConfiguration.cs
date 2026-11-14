using Capitra.Domain.Modules.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinBranchConfiguration : IEntityTypeConfiguration<FinBranch>
{
    public void Configure(EntityTypeBuilder<FinBranch> builder)
    {
        builder.ToTable("FIN_BRANCHES");
        builder.HasKey(x => x.BranchId);
        builder.HasIndex(x => new { x.LegalEntityId, x.BranchCode }).IsUnique();
        builder.HasIndex(x => new { x.LegalEntityId, x.IsActive });
        builder.HasIndex(x => x.ParentBranchId);

        builder.Property(x => x.BranchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BranchCode).HasMaxLength(30);
        builder.Property(x => x.BranchNameEn).HasMaxLength(150);
        builder.Property(x => x.BranchNameAr).HasMaxLength(150);
        builder.Property(x => x.BranchType).HasMaxLength(30);
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.CityName).HasMaxLength(120);
        builder.Property(x => x.AddressEn).HasMaxLength(500);
        builder.Property(x => x.AddressAr).HasMaxLength(500);
        builder.Property(x => x.PhoneNo).HasMaxLength(50);
        builder.Property(x => x.EmailAddress).HasMaxLength(150);

        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.ParentBranchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
