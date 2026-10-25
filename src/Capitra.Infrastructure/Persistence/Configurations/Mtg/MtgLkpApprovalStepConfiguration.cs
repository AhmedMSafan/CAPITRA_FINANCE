using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpApprovalStepConfiguration : LookupConfiguration<MtgLkpApprovalStep>
{
    protected override string TableName => "MTG_LKP_APPROVAL_STEPS";
}
