using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqLkpIssuerBankConfiguration : IEntityTypeConfiguration<ChqLkpIssuerBank>
{
    public void Configure(EntityTypeBuilder<ChqLkpIssuerBank> builder)
    {
        builder.ToTable("CHQ_LKP_ISSUER_BANKS");
        builder.HasKey(x => x.BankCode);

        builder.Property(x => x.BankCode).HasMaxLength(30);
        builder.Property(x => x.BankNameEn).HasMaxLength(300);
        builder.Property(x => x.BankNameAr).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
