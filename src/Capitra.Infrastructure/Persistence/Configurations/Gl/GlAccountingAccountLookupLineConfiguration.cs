using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingAccountLookupLineConfiguration : IEntityTypeConfiguration<GlAccountingAccountLookupLine>
{
    public void Configure(EntityTypeBuilder<GlAccountingAccountLookupLine> builder)
    {
        builder.ToTable("GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES");
        builder.HasKey(x => x.LookupLineId);
        builder.HasIndex(x => new { x.AccountLookupId, x.IsActive, x.PriorityNo });
        builder.HasIndex(x => new { x.BusinessLineCode, x.ProductCode, x.TransactionTypeCode });

        builder.Property(x => x.LookupLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.ProductCode).HasMaxLength(60);
        builder.Property(x => x.TransactionTypeCode).HasMaxLength(60);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CustomerTypeCode).HasMaxLength(60);
        builder.Property(x => x.PortfolioCode).HasMaxLength(60);
        builder.Property(x => x.FundingSourceCode).HasMaxLength(60);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.ResultAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultBusinessLineValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultBranchValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultCostCenterValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCostCenter>().WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.ResultAccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultCompanyValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultFuture1ValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ResultFuture2ValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingAccountLookup>().WithMany().HasForeignKey(x => x.AccountLookupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
