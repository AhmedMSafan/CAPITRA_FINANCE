using Capitra.Application.Modules.Trs;
using Xunit;

namespace Capitra.Tests.Trs;

/// <summary>Reverse factoring: each funding line is split over the suppliers by their share of the eligible invoices.</summary>
public sealed class TrsSupplierTransferSplitTests
{
    [Fact]
    public void Each_line_is_split_by_weight_and_adds_up_to_the_line()
    {
        var rows = TrsDisburseService.Split([1_000_000m, 333.33m], [0.5m, 0.3m, 0.2m]);
        Assert.Equal(6, rows.Count);
        Assert.Equal(1_000_000m, rows.Where(x => x.Line == 0).Sum(x => x.Amount));
        Assert.Equal(333.33m, rows.Where(x => x.Line == 1).Sum(x => x.Amount));
        Assert.Equal(500_000m, rows.Single(x => x is { Line: 0, Supplier: 0 }).Amount);
        Assert.Equal(300_000m, rows.Single(x => x is { Line: 0, Supplier: 1 }).Amount);
    }

    [Fact]
    public void The_last_supplier_takes_the_rounding()
    {
        var rows = TrsDisburseService.Split([100m], [1m, 1m, 1m]);
        Assert.Equal([33.33m, 33.33m, 33.34m], rows.Select(x => x.Amount));
    }

    [Fact]
    public void Zero_weights_split_evenly()
    {
        var rows = TrsDisburseService.Split([10m], [0m, 0m]);
        Assert.Equal([5m, 5m], rows.Select(x => x.Amount));
    }
}
