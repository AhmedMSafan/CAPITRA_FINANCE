using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctDisbursementChequeConfiguration : IEntityTypeConfiguration<FctDisbursementCheque>
{
    public void Configure(EntityTypeBuilder<FctDisbursementCheque> builder)
    {
        builder.ToTable("FCT_DISBURSEMENT_CHEQUES");
        builder.HasKey(x => x.DisbursementChequeId);
        builder.HasIndex(x => new { x.DisbursementId, x.StatusCode });
        builder.HasIndex(x => x.WalletChequeId);

        builder.Property(x => x.DisbursementChequeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.IssuerBankName).HasMaxLength(300);
        builder.Property(x => x.IssuerBankCode).HasMaxLength(30);
        builder.Property(x => x.IssuerBankBranch).HasMaxLength(200);
        builder.Property(x => x.DrawerNameEn).HasMaxLength(300);
        builder.Property(x => x.DrawerNameAr).HasMaxLength(300);
        builder.Property(x => x.DrawerNationalId).HasMaxLength(50);
        builder.Property(x => x.DrawerAccountNo).HasMaxLength(100);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CustomerId).HasMaxLength(100);
        builder.Property(x => x.CustomerNameEn).HasMaxLength(300);
        builder.Property(x => x.CustomerNameAr).HasMaxLength(300);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ChqCheque>().WithMany().HasForeignKey(x => x.WalletChequeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
