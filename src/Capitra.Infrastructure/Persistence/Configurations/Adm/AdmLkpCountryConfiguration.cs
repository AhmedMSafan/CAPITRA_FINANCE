using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpCountryConfiguration : LookupConfiguration<AdmLkpCountry>
{
    protected override string TableName => "ADM_LKP_COUNTRY";
}
