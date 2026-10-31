using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpLegalEntityConfiguration : LookupConfiguration<AdmLkpLegalEntity>
{
    protected override string TableName => "ADM_LKP_LEGAL_ENTITIES";

    protected override int NameMaxLength => 240;

    public override void Configure(EntityTypeBuilder<AdmLkpLegalEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.BaseCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.BaseCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpCountry>().WithMany().HasForeignKey(x => x.CountryCode).OnDelete(DeleteBehavior.Restrict);
    }
}
