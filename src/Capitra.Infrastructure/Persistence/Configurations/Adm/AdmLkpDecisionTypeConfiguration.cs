using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDecisionTypeConfiguration : LookupConfiguration<AdmLkpDecisionType>
{
    protected override string TableName => "ADM_LKP_DECISION_TYPES";
}
