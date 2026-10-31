using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffContractConfiguration : IEntityTypeConfiguration<MffContract>
{
    public void Configure(EntityTypeBuilder<MffContract> builder)
    {
        builder.ToTable("MFF_CONTRACTS");
        builder.HasKey(x => x.ContractId);
        builder.HasIndex(x => x.ContractNo).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.BuyerPersonId);
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.FinancierId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.SellerPersonId);

        builder.Property(x => x.ContractId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ContractNo).HasMaxLength(100);
        builder.Property(x => x.ContractType).HasMaxLength(50);
        builder.Property(x => x.FinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.PropertyPrice).HasPrecision(18, 2);
        builder.Property(x => x.ContractStatus).HasMaxLength(30);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.BuyerPersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplicationDocument>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProperty>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.SellerPersonId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
