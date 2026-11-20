using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsBankAccountConfiguration : IEntityTypeConfiguration<TrsBankAccount>
{
    public void Configure(EntityTypeBuilder<TrsBankAccount> builder)
    {
        builder.ToTable("TRS_BANK_ACCOUNTS");
        builder.HasKey(x => x.AccountId);
        builder.HasIndex(x => new { x.CompanyId, x.AccountRef }).IsUnique();
        builder.HasIndex(x => new { x.BankId, x.AccountNo }).IsUnique();
        builder.HasIndex(x => x.Iban);
        builder.HasIndex(x => new { x.CompanyId, x.BankId, x.AccountTypeCode });

        builder.Property(x => x.AccountId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountRef).HasMaxLength(50);
        builder.Property(x => x.AccountNo).HasMaxLength(80);
        builder.Property(x => x.Iban).HasMaxLength(40);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.AccountTypeCode).HasMaxLength(50);
        builder.Property(x => x.PurposeEn).HasMaxLength(240);
        builder.Property(x => x.PurposeAr).HasMaxLength(240);
        builder.Property(x => x.ErpAccountCode).HasMaxLength(80);
        builder.Property(x => x.CurrentBalance).HasPrecision(18, 3);

        // COMPANY_ID -> TRS_COMPANIES: added when that table is ported.
        builder.HasOne<TrsBank>().WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsBankAccount>().WithMany().HasForeignKey(x => x.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
        // CURRENCY_CODE -> TRS_CURRENCIES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
