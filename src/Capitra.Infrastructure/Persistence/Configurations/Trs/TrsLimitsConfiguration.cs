using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsFacilityTrancheConfiguration : IEntityTypeConfiguration<TrsFacilityTranche>
{
    public void Configure(EntityTypeBuilder<TrsFacilityTranche> builder)
    {
        builder.ToTable("TRS_FACILITY_TRANCHES");
        builder.HasKey(x => x.TrancheId);
        builder.HasIndex(x => new { x.FacilityId, x.TrancheName }).IsUnique();

        builder.Property(x => x.TrancheId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TrancheName).HasMaxLength(100);
        builder.Property(x => x.TrancheAmount).HasPrecision(18, 3);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<TrsCreditFacility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class TrsFacilitySublimitConfiguration : IEntityTypeConfiguration<TrsFacilitySublimit>
{
    public void Configure(EntityTypeBuilder<TrsFacilitySublimit> builder)
    {
        builder.ToTable("TRS_FACILITY_SUBLIMITS");
        builder.HasKey(x => x.SublimitId);
        builder.HasIndex(x => new { x.TrancheId, x.SublimitName }).IsUnique();
        builder.HasIndex(x => x.FacilityId);

        builder.Property(x => x.SublimitId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SublimitName).HasMaxLength(100);
        builder.Property(x => x.LimitAmount).HasPrecision(18, 3);
        builder.Property(x => x.PoolTypeCode).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<TrsCreditFacility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsFacilityTranche>().WithMany().HasForeignKey(x => x.TrancheId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class TrsSublimitScopeConfiguration : IEntityTypeConfiguration<TrsSublimitScope>
{
    public void Configure(EntityTypeBuilder<TrsSublimitScope> builder)
    {
        builder.ToTable("TRS_SUBLIMIT_SCOPES");
        builder.HasKey(x => x.ScopeId);
        builder.HasIndex(x => new { x.SublimitId, x.ScopeTypeCode, x.ScopeValue }).IsUnique();

        builder.Property(x => x.ScopeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ScopeTypeCode).HasMaxLength(30);
        builder.Property(x => x.ScopeValue).HasMaxLength(50);

        builder.HasOne<TrsFacilitySublimit>().WithMany().HasForeignKey(x => x.SublimitId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class TrsBankRuleConfiguration : IEntityTypeConfiguration<TrsBankRule>
{
    public void Configure(EntityTypeBuilder<TrsBankRule> builder)
    {
        builder.ToTable("TRS_BANK_RULES");
        builder.HasKey(x => x.RuleId);
        builder.HasIndex(x => new { x.BankId, x.StatusCode });
        builder.HasIndex(x => x.FacilityId);

        builder.Property(x => x.RuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleTypeCode).HasMaxLength(30);
        builder.Property(x => x.SubjectTypeCode).HasMaxLength(30);
        builder.Property(x => x.SubjectValue).HasMaxLength(200);
        builder.Property(x => x.LimitPct).HasPrecision(9, 6);
        builder.Property(x => x.LimitAmount).HasPrecision(18, 3);
        builder.Property(x => x.WarningPct).HasPrecision(9, 6);
        builder.Property(x => x.BasisCode).HasMaxLength(30);
        builder.Property(x => x.BreachActionCode).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.ChangedBy).HasMaxLength(100);
        builder.Property(x => x.VerifiedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<TrsBank>().WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsCreditFacility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.SubjectClientId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class TrsSublimitMovementConfiguration : IEntityTypeConfiguration<TrsSublimitMovement>
{
    public void Configure(EntityTypeBuilder<TrsSublimitMovement> builder)
    {
        builder.ToTable("TRS_SUBLIMIT_MOVEMENTS");
        builder.HasKey(x => x.MovementId);
        builder.HasIndex(x => x.SublimitId);
        builder.HasIndex(x => x.DisbursementId);
        // no foreign key: resetting the portfolios truncates the fundings with the disbursements, and must not cascade into the opening balances
        builder.HasIndex(x => x.FundingId);
        builder.HasIndex(x => x.DebtorClientId);
        builder.HasIndex(x => x.SellerClientId);

        builder.Property(x => x.MovementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.MovementTypeCode).HasMaxLength(20);
        builder.Property(x => x.OutstandingChange).HasPrecision(18, 3);
        builder.Property(x => x.ReservedChange).HasPrecision(18, 3);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<TrsFacilitySublimit>().WithMany().HasForeignKey(x => x.SublimitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.DebtorClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.SellerClientId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class TrsDisbursementFundingConfiguration : IEntityTypeConfiguration<TrsDisbursementFunding>
{
    public void Configure(EntityTypeBuilder<TrsDisbursementFunding> builder)
    {
        builder.ToTable("TRS_DISBURSEMENT_FUNDINGS");
        builder.HasKey(x => x.FundingId);
        builder.HasIndex(x => new { x.DisbursementId, x.StatusCode });
        builder.HasIndex(x => x.SublimitId);

        builder.Property(x => x.FundingId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceTypeCode).HasMaxLength(20);
        builder.Property(x => x.FundedAmount).HasPrecision(18, 3);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.OverrideReason).HasMaxLength(1000);
        builder.Property(x => x.OverrideApprovedBy).HasMaxLength(100);
        builder.Property(x => x.ChangedBy).HasMaxLength(100);
        builder.Property(x => x.ExecutedBy).HasMaxLength(100);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsFacilitySublimit>().WithMany().HasForeignKey(x => x.SublimitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsCreditFacility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsBank>().WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsBankAccount>().WithMany().HasForeignKey(x => x.PaidFromAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class TrsSupplierTransferConfiguration : IEntityTypeConfiguration<TrsSupplierTransfer>
{
    public void Configure(EntityTypeBuilder<TrsSupplierTransfer> builder)
    {
        builder.ToTable("TRS_SUPPLIER_TRANSFERS");
        builder.HasKey(x => x.TransferId);
        builder.HasIndex(x => new { x.DisbursementId, x.StatusCode });

        builder.Property(x => x.TransferId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SupplierName).HasMaxLength(300);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.SupplierBankName).HasMaxLength(200);
        builder.Property(x => x.SupplierIban).HasMaxLength(50);
        builder.Property(x => x.SupplierAccountNo).HasMaxLength(50);
        builder.Property(x => x.Amount).HasPrecision(18, 3);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.SentBy).HasMaxLength(100);
        builder.Property(x => x.BankReference).HasMaxLength(100);
        builder.Property(x => x.ConfirmedBy).HasMaxLength(100);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsDisbursementFunding>().WithMany().HasForeignKey(x => x.FundingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.SupplierClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClientBankAccount>().WithMany().HasForeignKey(x => x.SupplierBankAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsBankAccount>().WithMany().HasForeignKey(x => x.PaidFromAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
