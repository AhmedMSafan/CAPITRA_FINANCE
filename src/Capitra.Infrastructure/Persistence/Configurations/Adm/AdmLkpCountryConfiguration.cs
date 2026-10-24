using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpCountryConfiguration : LookupConfiguration<AdmLkpCountry>
{
    protected override string TableName => "ADM_LKP_COUNTRY";

    protected override int NameMaxLength => 100;

    public override void Configure(EntityTypeBuilder<AdmLkpCountry> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Code).HasMaxLength(2);
    }
}
