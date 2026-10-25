using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDocumentGroupConfiguration : LookupConfiguration<AdmLkpDocumentGroup>
{
    protected override string TableName => "ADM_LKP_DOCUMENT_GROUPS";
}
