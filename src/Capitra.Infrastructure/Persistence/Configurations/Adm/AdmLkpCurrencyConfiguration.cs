using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpCurrencyConfiguration : LookupConfiguration<AdmLkpCurrency>
{
    protected override string TableName => "ADM_LKP_CURRENCY";

    protected override int NameMaxLength => 100;

    public override void Configure(EntityTypeBuilder<AdmLkpCurrency> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Code).HasMaxLength(3);
    }
}
