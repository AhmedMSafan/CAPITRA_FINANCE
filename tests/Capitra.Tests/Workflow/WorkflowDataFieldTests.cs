using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Infrastructure.Persistence.Migrations;

namespace Capitra.Tests.Workflow;

/// <summary>Data fields declared in the designer: read from older and newer diagrams, defaults filled, types and required fields checked.</summary>
public sealed class WorkflowDataFieldTests
{
    private const string Fields = """[{"name":"amount","label":"Amount","type":"Decimal","required":true},{"name":"risk","type":"Choice","choices":["LOW","HIGH"],"defaultValue":"LOW"},{"name":"urgent","type":"Boolean"},{"name":"due","type":"Date"},{"name":"count","type":"Number"}]""";

    private static string Xml(string json) =>
        $"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P" finova:variables="{System.Security.SecurityElement.Escape(json)}"/></bpmn:definitions>""";

    [Fact]
    public void Older_factoring_variables_read_as_fields()
    {
        var fields = WfeDataFields.Of(SeedFactoringWorkflow.FactoringBpmn);
        Assert.Contains(fields, f => f.Name == "entity_id" && f.Type == "Number" && f.Required);
    }

    [Fact]
    public void Start_fills_defaults_and_writes_values_in_one_form()
    {
        var fields = WfeDataFields.Of(Xml(Fields));
        var v = WfeDataFields.ForStart(fields, new Dictionary<string, string?> { ["AMOUNT"] = "1,250.50", ["urgent"] = "Y", ["due"] = "2026-11-08", ["module_code"] = "FACTORING" });
        Assert.Equal("1250.50", v["amount"]);
        Assert.Equal("LOW", v["risk"]);
        Assert.Equal("true", v["urgent"]);
        Assert.Equal("2026-11-08", v["due"]);
        Assert.Equal("FACTORING", v["module_code"]);
    }

    [Fact]
    public void Start_refuses_a_missing_required_field_or_a_wrong_type()
    {
        var fields = WfeDataFields.Of(Xml(Fields));
        var e = Assert.Throws<BusinessValidationException>(() => WfeDataFields.ForStart(fields, new Dictionary<string, string?> { ["risk"] = "MEDIUM", ["count"] = "1.5" }));
        Assert.Contains("amount", e.Errors.Keys);
        Assert.Contains("risk", e.Errors.Keys);
        Assert.Contains("count", e.Errors.Keys);
    }

    [Fact]
    public void A_workflow_without_fields_starts_as_before()
    {
        var v = WfeDataFields.ForStart([], new Dictionary<string, string?> { ["anything"] = "x" });
        Assert.Equal("x", v["anything"]);
    }
}
