using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmProductActionConfiguration : IEntityTypeConfiguration<AdmProductAction>
{
    public void Configure(EntityTypeBuilder<AdmProductAction> builder)
    {
        builder.ToTable("ADM_PRODUCT_ACTIONS");
        builder.HasKey(x => x.ProductActionId);
        builder.HasIndex(x => x.ActionCode).IsUnique();
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductEventRuleId);

        builder.Property(x => x.ProductActionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(50);

        builder.HasOne<AdmProduct>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmProductEventRule>().WithMany().HasForeignKey(x => x.ProductEventRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
