using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Domain.Common;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Capitra.Tests.Persistence;

/// <summary>The generic lookup API validates against LookupRegistry; these tests keep it in step with the tables.</summary>
public sealed class LookupRegistryTests
{
    private static readonly IModel Model = new CapitraDbContextFactory().CreateDbContext([]).GetService<IDesignTimeModel>().Model;

    public static TheoryData<string> LookupNames => [.. LookupRegistry.All.Keys];

    [Fact]
    public void Every_lookup_table_is_registered()
    {
        var tables = Model.GetEntityTypes().Where(e => typeof(LookupEntity).IsAssignableFrom(e.ClrType)).Select(e => e.ClrType);
        var registered = LookupRegistry.All.Values.Select(d => d.EntityType);
        Assert.Empty(tables.Except(registered));
    }

    [Theory]
    [MemberData(nameof(LookupNames))]
    public void Definition_matches_the_table(string lookupName)
    {
        var definition = LookupRegistry.All[lookupName];
        var entity = Model.FindEntityType(definition.EntityType)!;

        Assert.Equal(definition.CodeMaxLength, entity.FindProperty(nameof(LookupEntity.Code))!.GetMaxLength());
        Assert.Equal(definition.NameMaxLength, entity.FindProperty(nameof(LookupEntity.NameAr))!.GetMaxLength());
        Assert.Equal(definition.NameMaxLength, entity.FindProperty(nameof(LookupEntity.NameEn))!.GetMaxLength());

        foreach (var field in definition.Fields)
        {
            var property = entity.FindProperty(field.PropertyName);
            Assert.True(property is not null, $"{lookupName}: no column for field {field.Name}");
            if (field.MaxLength is { } max)
                Assert.Equal(max, property.GetMaxLength());
            if (field.Type == LookupFieldType.Lookup)
                Assert.True(LookupRegistry.All.ContainsKey(field.LookupName!), $"{lookupName}: unknown lookup {field.LookupName}");
        }
    }
}
