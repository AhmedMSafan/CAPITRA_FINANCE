using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_DATA_SOURCES: a list of values a workflow form can offer in a drop-down (K2's SmartObjects, kept simple): its own items
/// below. The built-in sources (users, clients, currencies) read the application's tables and have no row here.
/// </summary>
public class WfeDataSource : AuditableEntity
{
    public long DataSourceId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<WfeDataSourceItem> Items { get; set; } = [];
}

/// <summary>WFE_DATA_SOURCE_ITEMS: one value of a data source: what is stored and what people see.</summary>
public class WfeDataSourceItem : AuditableEntity
{
    public long DataSourceItemId { get; set; }

    public long DataSourceId { get; set; }

    public string ItemValue { get; set; } = string.Empty;

    public string ItemLabel { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
