using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmClientRoleConfiguration : IEntityTypeConfiguration<CrmClientRole>
{
    public void Configure(EntityTypeBuilder<CrmClientRole> builder)
    {
        builder.ToTable("CRM_CLIENT_ROLES");
        builder.HasKey(x => x.ClientRoleId);
        builder.HasIndex(x => new { x.ClientId, x.RoleCode }).IsUnique();
        builder.HasIndex(x => new { x.RoleCode, x.StatusCode });

        builder.Property(x => x.ClientRoleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RoleCode).HasMaxLength(30);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class CrmClientComplianceHistoryConfiguration : IEntityTypeConfiguration<CrmClientComplianceHistory>
{
    public void Configure(EntityTypeBuilder<CrmClientComplianceHistory> builder)
    {
        builder.ToTable("CRM_CLIENT_COMPLIANCE_HISTORY");
        builder.HasKey(x => x.ComplianceHistoryId);
        builder.HasIndex(x => x.ClientId);

        builder.Property(x => x.ComplianceHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(20);
        builder.Property(x => x.RiskGrade).HasMaxLength(20);
        builder.Property(x => x.KycStatus).HasMaxLength(30);
        builder.Property(x => x.AmlStatus).HasMaxLength(30);
        builder.Property(x => x.SanctionsStatus).HasMaxLength(30);
        builder.Property(x => x.BlacklistReason).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class CrmClientBankAccountConfiguration : IEntityTypeConfiguration<CrmClientBankAccount>
{
    public void Configure(EntityTypeBuilder<CrmClientBankAccount> builder)
    {
        builder.ToTable("CRM_CLIENT_BANK_ACCOUNTS");
        builder.HasKey(x => x.BankAccountId);
        builder.HasIndex(x => new { x.ClientId, x.CurrencyId });
        builder.HasIndex(x => x.Iban);

        builder.Property(x => x.BankAccountId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BankName).HasMaxLength(200);
        builder.Property(x => x.BranchName).HasMaxLength(200);
        builder.Property(x => x.SwiftCode).HasMaxLength(11);
        builder.Property(x => x.AccountNumber).HasMaxLength(50);
        builder.Property(x => x.Iban).HasMaxLength(34);
        builder.Property(x => x.AccountHolderName).HasMaxLength(300);
        builder.Property(x => x.AccountHolderNameAr).HasMaxLength(300);
        builder.Property(x => x.AccountTypeCode).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.ChangedBy).HasMaxLength(255);
        builder.Property(x => x.VerifiedBy).HasMaxLength(255);
        builder.Property(x => x.VerificationMethodCode).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class CrmClientBankAccountHistoryConfiguration : IEntityTypeConfiguration<CrmClientBankAccountHistory>
{
    public void Configure(EntityTypeBuilder<CrmClientBankAccountHistory> builder)
    {
        builder.ToTable("CRM_CLIENT_BANK_ACCOUNT_HISTORY");
        builder.HasKey(x => x.BankAccountHistoryId);
        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.ClientId);

        builder.Property(x => x.BankAccountHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(20);
        builder.Property(x => x.BankName).HasMaxLength(200);
        builder.Property(x => x.BranchName).HasMaxLength(200);
        builder.Property(x => x.SwiftCode).HasMaxLength(11);
        builder.Property(x => x.AccountNumber).HasMaxLength(50);
        builder.Property(x => x.Iban).HasMaxLength(34);
        builder.Property(x => x.AccountHolderName).HasMaxLength(300);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.VerificationMethodCode).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<CrmClientBankAccount>().WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
