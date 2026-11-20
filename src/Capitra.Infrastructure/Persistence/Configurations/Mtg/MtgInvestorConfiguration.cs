using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgInvestorConfiguration : IEntityTypeConfiguration<MtgInvestor>
{
    public void Configure(EntityTypeBuilder<MtgInvestor> builder)
    {
        builder.ToTable("MTG_INVESTORS");
        builder.HasKey(x => x.InvestorId);
        builder.HasIndex(x => x.InvestorCode).IsUnique();

        builder.Property(x => x.InvestorId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InvestorCode).HasMaxLength(30);
        builder.Property(x => x.FullNameAr).HasMaxLength(300);
        builder.Property(x => x.FullNameEn).HasMaxLength(300);
        builder.Property(x => x.NationalId).HasMaxLength(50);
        builder.Property(x => x.PhoneNo).HasMaxLength(100);
        builder.Property(x => x.EmailAddress).HasMaxLength(320);
        builder.Property(x => x.AddressText).HasMaxLength(1000);
        builder.Property(x => x.EmployerName).HasMaxLength(300);
        builder.Property(x => x.DeclaredIncome).HasPrecision(18, 2);

        builder.ConfigureAudit();
    }
}
