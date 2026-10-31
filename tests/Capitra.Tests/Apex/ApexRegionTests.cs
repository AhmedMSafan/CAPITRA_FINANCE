using Capitra.Application.Modules.Gl.Pages;

namespace Capitra.Tests.Apex;

public sealed class ApexRegionTests
{
    [Fact]
    public void Every_region_column_is_a_property_of_its_entity()
    {
        var missing = GlApexRegions.All
            .SelectMany(r => r.Table.Columns.Where(c => r.Table.Entity.GetProperty(c.Property) is null)
                .Select(c => $"{r.Table.Name}.{c.Name} -> {r.Table.Entity.Name}.{c.Property}"))
            .Distinct()
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void Every_region_has_its_primary_key_and_parent_column()
    {
        foreach (var r in GlApexRegions.All)
        {
            Assert.Contains(r.Table.Columns, c => c.Name == r.Table.Pk);
            if (r.ParentColumn is not null)
                Assert.Contains(r.Table.Columns, c => c.Name == r.ParentColumn);
        }
    }

    [Fact]
    public void Every_select_list_reads_existing_columns()
    {
        var unknown = GlApexRegions.Lovs.SelectMany(l => new[] { l.Display, l.Return }.Where(n => l.Table.Columns.All(c => c.Name != n))
            .Select(n => $"{l.Item}: {l.Table.Name}.{n}")).ToList();
        Assert.Empty(unknown);

        var missing = GlApexRegions.Lovs.SelectMany(l => l.Table.Columns.Where(c => l.Table.Entity.GetProperty(c.Property) is null)
            .Select(c => $"{l.Table.Name}.{c.Name}")).Distinct().ToList();
        Assert.Empty(missing);
    }
}
