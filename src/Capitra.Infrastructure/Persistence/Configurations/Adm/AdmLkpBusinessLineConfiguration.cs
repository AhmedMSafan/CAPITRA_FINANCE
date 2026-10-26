using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpBusinessLineConfiguration : LookupConfiguration<AdmLkpBusinessLine>
{
    protected override string TableName => "ADM_LKP_BUSINESS_LINES";

    protected override int NameMaxLength => 240;
}
