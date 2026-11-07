using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffPersonConfiguration : IEntityTypeConfiguration<MffPerson>
{
    public void Configure(EntityTypeBuilder<MffPerson> builder)
    {
        builder.ToTable("MFF_PERSONS");
        builder.HasKey(x => x.PersonId);
        builder.HasIndex(x => x.NationalId).IsUnique();
        builder.HasIndex(x => x.EmploymentTypeCode);
        builder.HasIndex(x => x.MaritalStatusCode);

        builder.Property(x => x.PersonId).UseIdentityByDefaultColumn();
        builder.Property(x => x.NationalId).HasMaxLength(30);
        builder.Property(x => x.FullNameAr).HasMaxLength(300);
        builder.Property(x => x.GenderCode).HasMaxLength(1);
        builder.Property(x => x.MaritalStatusCode).HasMaxLength(50);
        builder.Property(x => x.MobileNo).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.AddressAr).HasMaxLength(1000);
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(50);
        builder.Property(x => x.EmployerName).HasMaxLength(300);
        builder.Property(x => x.TaxNo).HasMaxLength(100);
        builder.Property(x => x.CommercialRegNo).HasMaxLength(100);

        builder.HasOne<MffLkpEmploymentType>().WithMany().HasForeignKey(x => x.EmploymentTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpMaritalStatus>().WithMany().HasForeignKey(x => x.MaritalStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
