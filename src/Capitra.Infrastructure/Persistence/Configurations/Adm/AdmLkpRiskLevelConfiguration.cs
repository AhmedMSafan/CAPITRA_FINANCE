using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpRiskLevelConfiguration : LookupConfiguration<AdmLkpRiskLevel>
{
    protected override string TableName => "ADM_LKP_RISK_LEVEL";

    protected override int NameMaxLength => 100;
}
