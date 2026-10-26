using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsBankConfiguration : IEntityTypeConfiguration<TrsBank>
{
    public void Configure(EntityTypeBuilder<TrsBank> builder)
    {
        builder.ToTable("TRS_BANKS");
        builder.HasKey(x => x.BankId);
        builder.HasIndex(x => new { x.CompanyId, x.BankCode }).IsUnique();
        builder.HasIndex(x => x.SwiftBic).IsUnique();
        builder.HasIndex(x => x.BankNameAr);

        builder.Property(x => x.BankId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BankCode).HasMaxLength(30);
        builder.Property(x => x.BankNameEn).HasMaxLength(240);
        builder.Property(x => x.BankNameAr).HasMaxLength(240);
        builder.Property(x => x.CbeCode).HasMaxLength(30);
        builder.Property(x => x.SwiftBic).HasMaxLength(20);
        builder.Property(x => x.FraRegistrationNo).HasMaxLength(80);
        builder.Property(x => x.RelationshipManagerName).HasMaxLength(200);
        builder.Property(x => x.RelationshipManagerEmail).HasMaxLength(320);
        builder.Property(x => x.RelationshipManagerPhone).HasMaxLength(50);
        builder.Property(x => x.KycStatus).HasMaxLength(20);

        // COMPANY_ID -> TRS_COMPANIES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
