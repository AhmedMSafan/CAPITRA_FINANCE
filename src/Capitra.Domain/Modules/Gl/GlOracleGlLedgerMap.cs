using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_LEDGER_MAPS: copy of FINOVA GL_ORACLE_GL_LEDGER_MAP (APEX), all columns kept.
/// </summary>
public class GlOracleGlLedgerMap : AuditableEntity
{
    public long LedgerMapId { get; set; }

    public long ExternalSystemId { get; set; }

    public long LedgerId { get; set; }

    public long? OracleLedgerId { get; set; }

    public string? OracleLedgerName { get; set; }

    public long? OracleSetOfBooksId { get; set; }

    public long? OracleChartOfAccountsId { get; set; }

    public string? OracleLedgerCurrency { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;
}
