using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>
/// Documents and workflow types: a document has its own fields and belongs to no workflow; a user-defined workflow is started by a
/// user on its one document and approvers see it read-only; a system-built workflow is never offered and links documents on steps.
/// </summary>
public sealed class WorkflowDocumentTests
{
    private static string Xml(string? type, string? document, string? stepForm = null) =>
        $"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P" {(type is null ? "" : $"finova:workflowType=\"{type}\"")} {(document is null ? "" : $"finova:documentCode=\"{document}\"")}><bpmn:startEvent id="S" name="Start"/><bpmn:userTask id="T" name="Manager approval" finova:stepCode="APPROVAL" finova:assignmentType="User" finova:assignee="ahmed" finova:actions="APPROVE,REJECT" {(stepForm is null ? "" : $"finova:formCode=\"{stepForm}\"")}/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="T"/><bpmn:sequenceFlow id="F2" sourceRef="T" targetRef="E"/></bpmn:process></bpmn:definitions>""";

    private sealed class User : ICurrentUser
    {
        public string? UserId => "1";
        public string? UserName => "ahmed";
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles => [];
    }

    private sealed class Clock : IClock
    {
        public DateTime UtcNow => new(2026, 11, 9, 10, 0, 0, DateTimeKind.Utc);
    }

    private sealed class Directory : IUserDirectory
    {
        public Task<IReadOnlyList<DirectoryUser>> ActiveUsersAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DirectoryUser>>([new("1", "ahmed", "Ahmed Safan", [], "Credit", "E-1001"), new("2", "mona", "Mona Hassan", [])]);
    }

    private static readonly WfeDataField[] LeaveFields =
    [
        new("leaveType", "Leave type", "Choice", true, null, ["ANNUAL", "SICK"], null),
        new("fromDate", "From", "Date", true, null, [], null),
        new("days", "Days", "Number", true, null, [], null),
        new("reason", "Reason", "String", false, null, [], null),
    ];

    private static WfeFormControl Field(string name, bool required = false) => new("field", name, null, null, null, required, false, null, "full");

    private static SaveWfeFormRequest Document(string code, IReadOnlyList<WfeDataField> fields, params WfeFormControl[] controls) =>
        new(code, code, null, null, true, new WfeFormLayout(1, controls), 0, fields);

    private sealed record Ctx(CapitraDbContext Db, WfeForms Forms, WfeInbox Inbox, WfeDefinitions Definitions);

    private static Ctx Setup(params (string Code, string Xml)[] workflows)
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);
        foreach (var (code, xml) in workflows)
            db.Add(new WfeWorkflowDefinition { WorkflowCode = code, WorkflowName = code, DefinitionStatus = "released", BpmnXml = xml, CreatedBy = "test" });
        db.SaveChanges();
        var runtime = new WfeRuntime(db, new Clock(), [], []);
        var forms = new WfeForms(db, new User(), new WfeDataSources(db, new Directory()), runtime, new Clock());
        return new Ctx(db, forms, new WfeInbox(db, new User(), runtime, new Clock(), new Directory(), forms), new WfeDefinitions(db, new User(), new Clock()));
    }

    [Fact]
    public async Task A_document_has_its_own_checked_fields_and_belongs_to_no_workflow()
    {
        var c = Setup(("LEAVE", Xml("USER", "LEAVE_REQUEST")));
        await using var _db = c.Db;

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(null, Document("LEAVE_REQUEST",
            [new("1st", null, "String", false, null, [], null), new("kind", "Kind", "Choice", false, null, [], null), new("days", null, "Number", false, "two", [], null)]), default));
        var problems = string.Join(" | ", bad.Errors["fields"]);
        Assert.Contains("Field 1", problems);
        Assert.Contains("list its choices", problems);
        Assert.Contains("whole number", problems);

        var notAField = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", LeaveFields, Field("amount")), default));
        Assert.Contains("not a data field of this document", string.Join(" ", notAField.Errors["layout"]));

        var saved = await c.Forms.SaveAsync(null, Document("leave_request", LeaveFields, Field("leaveType"), Field("fromDate"), Field("days"), Field("reason")), default);
        Assert.True(saved.IsDocument);
        Assert.Null(saved.WorkflowDefinitionId);
        Assert.Equal(["leaveType", "fromDate", "days", "reason"], saved.Fields.Select(f => f.Name));
        Assert.Equal(["LEAVE (whole workflow)"], saved.UsedAt);
        await Assert.ThrowsAsync<BusinessRuleException>(() => c.Forms.DeleteAsync(saved.FormId, default));
    }

    [Fact]
    public async Task A_user_defined_workflow_starts_on_its_document_and_approvers_see_it_read_only()
    {
        var c = Setup(("LEAVE", Xml("USER", "LEAVE_REQUEST")), ("FACTORING", Xml("SYSTEM", null, "LEAVE_REQUEST")), ("OLD_STYLE", Xml(null, null)));
        await using var _db = c.Db;
        await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", LeaveFields, Field("leaveType"), Field("fromDate"), Field("days"), Field("reason")), default);

        // only the user-defined workflow is offered: never a system-built one, and not one without a document
        Assert.Equal(["LEAVE"], (await c.Forms.StartListAsync(default)).Select(x => x.WorkflowCode));
        await Assert.ThrowsAsync<BusinessRuleException>(() => c.Forms.StartFormAsync("FACTORING", default));

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.StartAsync("LEAVE", new(null, new Dictionary<string, string?> { ["reason"] = "Family" }), default));
        Assert.Contains("days", missing.Errors.Keys);
        var started = await c.Forms.StartAsync("leave", new(null, new Dictionary<string, string?>
        {
            ["leaveType"] = "annual", ["fromDate"] = "2026-12-01", ["days"] = "1,0", ["reason"] = "Family",
        }), default);
        Assert.Equal("Manager approval", started.NextStep);
        var vars = await c.Db.Set<WfeInstanceVariable>().Where(x => x.WorkflowInstanceId == started.InstanceId).ToDictionaryAsync(x => x.VariableName);
        Assert.Equal("ANNUAL", vars["leaveType"].VariableValue);
        Assert.Equal("10", vars["days"].VariableValue);
        Assert.Equal("Date", vars["fromDate"].VariableType);

        // the approver sees the requester's document, read-only, even while working on the task
        var inboxId = started.InboxId!.Value;
        await c.Inbox.ClaimAsync(inboxId, default);
        await c.Inbox.StartAsync(inboxId, default);
        var view = (await c.Inbox.FormAsync(inboxId, default))!;
        Assert.False(view.Editable);
        Assert.Equal("ANNUAL", view.Controls.First(x => x.Field == "leaveType").Value);
        var data = await c.Inbox.DataAsync(inboxId, default);
        Assert.Contains(data.Fields, f => f.Name == "days" && f.Label == "Days");

        // approving keeps the document as the requester sent it
        await c.Inbox.CompleteAsync(inboxId, new("APPROVE", "Enjoy", Values: new Dictionary<string, string?> { ["days"] = "30" }), default);
        Assert.Equal("10", (await c.Db.Set<WfeInstanceVariable>().AsNoTracking().FirstAsync(x => x.WorkflowInstanceId == started.InstanceId && x.VariableName == "days")).VariableValue);
    }

    [Fact]
    public async Task A_user_defined_workflow_is_released_only_with_its_document()
    {
        var c = Setup();
        await using var _db = c.Db;
        SaveWfeDefinitionRequest Req(string xml, string status) => new("VACATION", "Vacation request", null, status, xml, null, true, 0);

        var e = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Definitions.SaveAsync(null, Req(Xml("USER", null), "released"), default));
        Assert.Contains("documentCode", e.Errors.Keys);
        await Assert.ThrowsAsync<BusinessValidationException>(() => c.Definitions.SaveAsync(null, Req(Xml("USER", "NOPE"), "released"), default));

        // a draft can be saved without it, and a system-built workflow never needs one
        var draft = await c.Definitions.SaveAsync(null, Req(Xml("USER", null), "draft"), default);
        Assert.Equal("draft", draft.DefinitionStatus);
        await c.Definitions.SaveAsync(null, Req(Xml("SYSTEM", null), "released") with { WorkflowCode = "SYS" }, default);
    }

    [Fact]
    public async Task Automatic_fields_take_the_signed_in_user_and_today_and_cannot_be_typed()
    {
        var c = Setup(("LEAVE", Xml("USER", "LEAVE_REQUEST")));
        await using var _db = c.Db;
        WfeDataField[] fields =
        [
            new("requester", "Requester", "User", false, null, [], null, "current_user"),
            new("requesterName", "Name", "String", false, null, [], null, WfeDataFields.CurrentUserName),
            new("department", "Department", "String", false, null, [], null, WfeDataFields.CurrentDepartment),
            new("requestDate", "Request date", "Date", false, null, [], null, WfeDataFields.Today),
            new("days", "Days", "Number", true, null, [], null),
        ];

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(null, Document("LEAVE_REQUEST",
            [new("days", null, "Number", false, null, [], null, WfeDataFields.Today)]), default));
        Assert.Contains("needs a Date field", string.Join(" ", bad.Errors["fields"]));

        var saved = await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", fields, Field("requester"), Field("requesterName"), Field("department"), Field("requestDate"), Field("days")), default);
        Assert.Equal(WfeDataFields.CurrentUser, saved.Fields[0].Auto);

        var form = await c.Forms.StartFormAsync("LEAVE", default);
        var requester = form.Controls.First(x => x.Field == "requester");
        Assert.True(requester.ReadOnly);
        Assert.Equal("ahmed", requester.Value);
        Assert.Equal("2026-11-09", form.Controls.First(x => x.Field == "requestDate").Value);

        // what the browser sends for an automatic field is ignored
        var started = await c.Forms.StartAsync("LEAVE", new(null, new Dictionary<string, string?> { ["requester"] = "mona", ["days"] = "2" }), default);
        var vars = await c.Db.Set<WfeInstanceVariable>().Where(x => x.WorkflowInstanceId == started.InstanceId).ToDictionaryAsync(x => x.VariableName, x => x.VariableValue);
        Assert.Equal(("ahmed", "Ahmed Safan", "Credit", "2026-11-09"), (vars["requester"], vars["requesterName"], vars["department"], vars["requestDate"]));
    }
}
