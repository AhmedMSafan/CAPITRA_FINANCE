using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmProductEventRuleConfiguration : IEntityTypeConfiguration<AdmProductEventRule>
{
    public void Configure(EntityTypeBuilder<AdmProductEventRule> builder)
    {
        builder.ToTable("ADM_PRODUCT_EVENT_RULES");
        builder.HasKey(x => x.ProductEventRuleId);
        builder.HasIndex(x => new { x.ProductId, x.EventTypeId, x.RuleCode }).IsUnique();
        builder.HasIndex(x => x.EventTypeId);

        builder.Property(x => x.ProductEventRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.RuleNameEn).HasMaxLength(200);
        builder.Property(x => x.RuleNameAr).HasMaxLength(200);

        builder.HasOne<AdmProduct>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEventType>().WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Charges).WithOne().HasForeignKey(x => x.ProductEventRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.GlLines).WithOne().HasForeignKey(x => x.ProductEventRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
