using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpInsuranceTypeConfiguration : LookupConfiguration<AdmLkpInsuranceType>
{
    protected override string TableName => "ADM_LKP_INSURANCE_TYPE";
}
