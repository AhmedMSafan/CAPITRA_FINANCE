using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        builder.ToTable("GL_ACCOUNTS");
        builder.HasKey(x => x.AccountId);
        builder.HasIndex(x => new { x.CoaId, x.AccountCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.AccountType, x.IsActive });
        builder.HasIndex(x => x.ParentAccountId);
        builder.HasIndex(x => x.AccountSegmentValueId);
        builder.HasIndex(x => new { x.FinancialStatementClass, x.CashFlowClass });

        builder.Property(x => x.AccountId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountCode).HasMaxLength(30);
        builder.Property(x => x.AccountNameEn).HasMaxLength(240);
        builder.Property(x => x.AccountNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.AccountType).HasMaxLength(30);
        builder.Property(x => x.NormalBalance).HasMaxLength(2);
        builder.Property(x => x.FinancialStatementClass).HasMaxLength(30);
        builder.Property(x => x.CashFlowClass).HasMaxLength(30);
        builder.Property(x => x.ExternalGlAccountMap).HasMaxLength(50);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.AccountSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
