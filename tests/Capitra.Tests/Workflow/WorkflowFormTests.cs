using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>Forms and data sources: a form is checked against the workflow's data fields, keeps to its own fields, and starts a workflow.</summary>
public sealed class WorkflowFormTests
{
    private const string Fields =
        """[{"name":"amount","label":"Amount","type":"Decimal","required":true},{"name":"purpose","type":"String"},{"name":"risk","type":"Choice","choices":["LOW","HIGH"]},{"name":"urgent","type":"Boolean"},{"name":"approver","type":"User"}]""";

    private static string Xml(string? startForm = "START_FORM", string? taskForm = "REVIEW_FORM") =>
        $"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P" finova:variables="{System.Security.SecurityElement.Escape(Fields)}"><bpmn:startEvent id="S" name="Start" {(startForm is null ? "" : $"finova:formCode=\"{startForm}\"")}/><bpmn:userTask id="T" name="Review" finova:stepCode="REVIEW" finova:assignmentType="Role" finova:assignee="CHECKER" finova:actions="APPROVE,REJECT" {(taskForm is null ? "" : $"finova:formCode=\"{taskForm}\"")}/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="T"/><bpmn:sequenceFlow id="F2" sourceRef="T" targetRef="E"/></bpmn:process></bpmn:definitions>""";

    private sealed class User : ICurrentUser
    {
        public string? UserId => "1";
        public string? UserName => "ahmed";
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles => [];
    }

    private sealed class Clock : IClock
    {
        public DateTime UtcNow => new(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);
    }

    private sealed class Directory : IUserDirectory
    {
        public Task<IReadOnlyList<DirectoryUser>> ActiveUsersAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DirectoryUser>>([new("1", "ahmed", "Ahmed Safan", []), new("2", "mona", "Mona Hassan", [])]);
    }

    private static (CapitraDbContext Db, WfeForms Forms, WfeDataSources Sources, long DefinitionId) Setup(string? startForm = "START_FORM")
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var def = new WfeWorkflowDefinition
        {
            WorkflowCode = "DISB", WorkflowName = "Disbursement", DefinitionStatus = "released", BpmnXml = Xml(startForm), CreatedBy = "test",
        };
        db.Add(def);
        db.Add(new WfeDataSource
        {
            SourceCode = "PURPOSES", SourceName = "Purposes", CreatedBy = "test",
            Items = [new() { ItemValue = "CAPEX", ItemLabel = "Capital spending", CreatedBy = "test" }, new() { ItemValue = "OPEX", ItemLabel = "Running costs", CreatedBy = "test" }],
        });
        db.SaveChanges();
        var sources = new WfeDataSources(db, new Directory());
        var runtime = new WfeRuntime(db, new Clock(), [], []);
        return (db, new WfeForms(db, new User(), sources, runtime), sources, def.WorkflowDefinitionId);
    }

    private static WfeFormControl Field(string name, string? control = null, string? source = null, bool required = false, bool readOnly = false) =>
        new("field", name, null, control, source, required, readOnly, null, "full");

    private static SaveWfeFormRequest Form(long defId, string code, params WfeFormControl[] controls) =>
        new(code, code, defId, null, true, new WfeFormLayout(1, controls), 0);

    [Fact]
    public async Task A_form_is_checked_against_the_workflow_data_fields()
    {
        var (db, forms, _, defId) = Setup();
        await using var _db = db;
        var e = await Assert.ThrowsAsync<BusinessValidationException>(() => forms.SaveAsync(null, Form(defId, "BAD_FORM",
            Field("nothing"), Field("amount", "date"), Field("purpose", "dropdown"), Field("purpose", "dropdown", "MISSING")), default));
        var problems = string.Join(" | ", e.Errors["layout"]);
        Assert.Contains("\"nothing\" is not a data field", problems);
        Assert.Contains("Decimal field", problems);
        Assert.Contains("choose the data source", problems);
        Assert.Contains("MISSING does not exist", problems);

        var ok = await forms.SaveAsync(null, Form(defId, "REVIEW_FORM", Field("amount", readOnly: true), Field("purpose", "dropdown", "purposes", required: true), Field("risk")), default);
        Assert.Equal(["Review"], ok.UsedAt);
        Assert.Equal("PURPOSES", ok.Layout.Controls[1].DataSource);
        Assert.Equal("dropdown", ok.Layout.Controls[2].Control);
    }

    [Fact]
    public async Task A_form_keeps_to_its_editable_fields_and_listed_values()
    {
        var (db, forms, _, defId) = Setup();
        await using var _db = db;
        await forms.SaveAsync(null, Form(defId, "REVIEW_FORM", Field("amount", readOnly: true), Field("purpose", "dropdown", "PURPOSES", required: true), Field("approver")), default);
        var form = (await forms.FindAsync(defId, "review_form", default))!;
        var fields = WfeDataFields.Of(Xml());

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => forms.AcceptAsync(form, fields, new Dictionary<string, string?> { ["purpose"] = "TRAVEL", ["approver"] = "nobody" }, default));
        Assert.Contains("purpose", bad.Errors.Keys);
        Assert.Contains("approver", bad.Errors.Keys);
        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => forms.AcceptAsync(form, fields, new Dictionary<string, string?>(), default));
        Assert.Contains("purpose", missing.Errors.Keys);

        // a read-only field and a value the form does not show are not taken from the browser
        var v = await forms.AcceptAsync(form, fields, new Dictionary<string, string?> { ["amount"] = "1", ["purpose"] = "capex", ["approver"] = "MONA", ["risk"] = "HIGH" }, default);
        Assert.Equal("CAPEX", v["purpose"]);
        Assert.Equal("mona", v["approver"]);
        Assert.False(v.ContainsKey("amount"));
        Assert.False(v.ContainsKey("risk"));
    }

    [Fact]
    public async Task A_workflow_with_a_start_form_starts_from_it()
    {
        var (db, forms, _, defId) = Setup();
        await using var _db = db;
        await forms.SaveAsync(null, Form(defId, "START_FORM", Field("amount", required: true), Field("urgent")), default);

        var list = await forms.StartListAsync(default);
        Assert.Equal("DISB", Assert.Single(list).WorkflowCode);

        await Assert.ThrowsAsync<BusinessValidationException>(() => forms.StartAsync("DISB", new(null, new Dictionary<string, string?> { ["urgent"] = "true" }), default));
        var started = await forms.StartAsync("disb", new("REQ-1", new Dictionary<string, string?> { ["amount"] = "1,500.25", ["urgent"] = "yes" }), default);
        var vars = await db.Set<WfeInstanceVariable>().Where(x => x.WorkflowInstanceId == started.InstanceId).ToDictionaryAsync(x => x.VariableName, x => x.VariableValue);
        Assert.Equal("1500.25", vars["amount"]);
        Assert.Equal("true", vars["urgent"]);
        Assert.Equal("Review", started.NextStep);
        Assert.Equal("CHECKER", started.NextAssignee);
    }

    [Fact]
    public async Task A_workflow_without_forms_is_not_offered_and_a_used_form_cannot_be_deleted()
    {
        var (db, forms, sources, defId) = Setup(startForm: null);
        await using var _db = db;
        Assert.Empty(await forms.StartListAsync(default));

        var saved = await forms.SaveAsync(null, Form(defId, "REVIEW_FORM", Field("purpose", "dropdown", "PURPOSES")), default);
        await Assert.ThrowsAsync<BusinessRuleException>(() => forms.DeleteAsync(saved.FormId, default));
        var purposes = await db.Set<WfeDataSource>().FirstAsync(x => x.SourceCode == "PURPOSES");
        await Assert.ThrowsAsync<BusinessRuleException>(() => sources.DeleteAsync(purposes.DataSourceId, default));

        Assert.Equal(2, (await sources.OptionsAsync("USERS", default)).Count);
        Assert.Equal(["CAPEX", "OPEX"], (await sources.OptionsAsync("purposes", default)).Select(x => x.Value));
    }
}
