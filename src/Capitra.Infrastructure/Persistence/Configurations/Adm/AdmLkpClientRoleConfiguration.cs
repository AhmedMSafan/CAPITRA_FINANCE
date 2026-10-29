using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpClientRoleConfiguration : LookupConfiguration<AdmLkpClientRole>
{
    protected override string TableName => "ADM_LKP_CLIENT_ROLES";
}
