using Capitra.Application.Modules.Wfe;
using Capitra.Infrastructure.Persistence.Migrations;

namespace Capitra.Tests.Workflow;

/// <summary>A new version is kept only when a workflow's steps or routing change, not when shapes move.</summary>
public sealed class WorkflowVersionTests
{
    private const string Xml = SeedFactoringWorkflow.FactoringBpmn;

    [Fact]
    public void Moving_the_canvas_or_formatting_is_the_same_process()
    {
        var moved = Xml.Replace("</bpmn:definitions>", "<!-- FINOVA_VIEWBOX:{\"x\":10,\"y\":20} -->\n</bpmn:definitions>");
        Assert.True(WfeVersions.SameProcess(Xml, moved));
    }

    [Fact]
    public void Moving_a_shape_is_the_same_process()
    {
        var i = Xml.IndexOf("<dc:Bounds x=\"", StringComparison.Ordinal);
        Assert.True(i > 0);
        var moved = Xml[..i] + "<dc:Bounds x=\"9" + Xml[(i + "<dc:Bounds x=\"".Length)..];
        Assert.True(WfeVersions.SameProcess(Xml, moved));
    }

    [Fact]
    public void Changing_a_step_or_a_flow_is_a_new_process()
    {
        Assert.False(WfeVersions.SameProcess(Xml, Xml.Replace("CLAIM,ACCEPT,REJECT,COMPLETE", "CLAIM,ACCEPT,COMPLETE")));
        Assert.False(WfeVersions.SameProcess(Xml, Xml.Replace("name=\"REJECT\"", "name=\"RETURN\"")));
    }
}
