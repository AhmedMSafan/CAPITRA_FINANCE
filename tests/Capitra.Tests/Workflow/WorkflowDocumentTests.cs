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
        Assert.Empty(await c.Forms.StartIssuesAsync(default));

        // a draft, or one without a document, is not offered, and designers are told why
        c.Db.Add(new WfeWorkflowDefinition { WorkflowCode = "DRAFT_WF", WorkflowName = "Draft wf", DefinitionStatus = "draft", BpmnXml = Xml("USER", "LEAVE_REQUEST"), CreatedBy = "test" });
        c.Db.Add(new WfeWorkflowDefinition { WorkflowCode = "NO_DOC", WorkflowName = "No doc", DefinitionStatus = "released", BpmnXml = Xml("USER", null), CreatedBy = "test" });
        await c.Db.SaveChangesAsync();
        Assert.Equal(["LEAVE"], (await c.Forms.StartListAsync(default)).Select(x => x.WorkflowCode));
        var issues = await c.Forms.StartIssuesAsync(default);
        Assert.Contains(issues, i => i.WorkflowCode == "DRAFT_WF" && i.Reason.Contains("draft"));
        Assert.Contains(issues, i => i.WorkflowCode == "NO_DOC" && i.Reason.Contains("No document"));
        Assert.Equal(2, issues.Count);
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
    public async Task A_required_workflow_field_the_document_lacks_blocks_release_and_is_explained_at_start()
    {
        // the workflow requires "amount", which the leave document does not have: nobody could ever fill it in
        var vars = "[{&quot;name&quot;:&quot;amount&quot;,&quot;label&quot;:&quot;Amount&quot;,&quot;type&quot;:&quot;Number&quot;,&quot;required&quot;:true}]";
        var xml = Xml("USER", "LEAVE_REQUEST").Replace("<bpmn:process id=\"P\"", $"<bpmn:process id=\"P\" finova:variables=\"{vars}\"");
        var c = Setup(("LEAVE", xml));
        await using var _db = c.Db;
        await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", LeaveFields, Field("leaveType"), Field("fromDate"), Field("days"), Field("reason")), default);
        Assert.Equal(["amount"], WfeForms.MissingOnDocument(xml, (await c.Db.Set<WfeForm>().FirstAsync()).FieldsJson).Select(f => f.Name));

        var release = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            c.Definitions.SaveAsync(null, new("LEAVE2", "Leave 2", null, "released", xml, null, true, 0), default));
        Assert.Contains("\"Amount\"", release.Errors["dataFields"][0]);

        var start = await Assert.ThrowsAsync<BusinessRuleException>(() => c.Forms.StartAsync("LEAVE", new(null, new Dictionary<string, string?>
        {
            ["leaveType"] = "ANNUAL", ["fromDate"] = "2026-12-01", ["days"] = "3",
        }), default));
        Assert.Contains("cannot be started yet", start.Message);
        Assert.Contains("no such fields", (await c.Forms.StartIssuesAsync(default)).Single().Reason);
    }

    [Fact]
    public async Task A_user_defined_workflow_rules_read_its_document_and_a_field_they_read_stays_on_the_document()
    {
        // a rule on "days" (a document field) is fine; one on "amount" (not on the document) is not
        static string WithRule(string field) => Xml("USER", "LEAVE_REQUEST").Replace(
            "<bpmn:sequenceFlow id=\"F2\" sourceRef=\"T\" targetRef=\"E\"/>",
            $"<bpmn:sequenceFlow id=\"F2\" sourceRef=\"T\" targetRef=\"E\" finova:conditions=\"[{{&quot;field&quot;:&quot;{field}&quot;,&quot;op&quot;:&quot;&gt;&quot;,&quot;value&quot;:&quot;5&quot;}}]\"/><bpmn:sequenceFlow id=\"F3\" sourceRef=\"T\" targetRef=\"E\"/>");
        var c = Setup();
        await using var _db = c.Db;
        var doc = await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", LeaveFields, Field("leaveType"), Field("fromDate"), Field("days"), Field("reason")), default);

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            c.Definitions.SaveAsync(null, new("LEAVE", "Leave", null, "released", WithRule("amount"), null, true, 0), default));
        Assert.Contains("no data field 'amount'", bad.Errors["rules"][0]);
        await c.Definitions.SaveAsync(null, new("LEAVE", "Leave", null, "released", WithRule("days"), null, true, 0), default);

        // "days" cannot leave the document while the released workflow's rule reads it; "reason" can
        var saved = await c.Db.Set<WfeForm>().AsNoTracking().FirstAsync(x => x.FormId == doc.FormId);
        var noDays = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(doc.FormId,
            Document("LEAVE_REQUEST", [LeaveFields[0], LeaveFields[1], LeaveFields[3]], Field("leaveType"), Field("fromDate"), Field("reason")) with { RowVersion = saved.RowVersion }, default));
        Assert.Contains("used by a rule in Leave", noDays.Errors["fields"][0]);
        await c.Forms.SaveAsync(doc.FormId,
            Document("LEAVE_REQUEST", [LeaveFields[0], LeaveFields[1], LeaveFields[2]], Field("leaveType"), Field("fromDate"), Field("days")) with { RowVersion = saved.RowVersion }, default);
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
            new("requestDate", "Request date", "Date", false, "@TodayDate", [], null),
            new("createdBy", "Created by", "User", false, "@UserName", [], null),
            new("days", "Days", "Number", true, null, [], null),
        ];

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(null, Document("LEAVE_REQUEST",
            [new("days", null, "Number", false, null, [], null, WfeDataFields.Today)]), default));
        Assert.Contains("needs a Date field", string.Join(" ", bad.Errors["fields"]));

        var saved = await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", fields, Field("requester"), Field("requesterName"), Field("department"), Field("requestDate"), Field("createdBy"), Field("days")), default);
        Assert.Equal(WfeDataFields.CurrentUser, saved.Fields[0].Auto);
        // typed tokens become automatic values
        Assert.Equal((WfeDataFields.Today, (string?)null), (saved.Fields[3].Auto, saved.Fields[3].DefaultValue));
        Assert.Equal(WfeDataFields.CurrentUser, saved.Fields[4].Auto);

        var form = await c.Forms.StartFormAsync("LEAVE", default);
        var requester = form.Controls.First(x => x.Field == "requester");
        Assert.True(requester.ReadOnly);
        Assert.Equal("ahmed", requester.Value);
        Assert.Equal("2026-11-09", form.Controls.First(x => x.Field == "requestDate").Value);

        // what the browser sends for an automatic field is ignored
        var started = await c.Forms.StartAsync("LEAVE", new(null, new Dictionary<string, string?> { ["requester"] = "mona", ["days"] = "2" }), default);
        var vars = await c.Db.Set<WfeInstanceVariable>().Where(x => x.WorkflowInstanceId == started.InstanceId).ToDictionaryAsync(x => x.VariableName, x => x.VariableValue);
        Assert.Equal(("ahmed", "Ahmed Safan", "Credit", "2026-11-09", "ahmed"), (vars["requester"], vars["requesterName"], vars["department"], vars["requestDate"], vars["createdBy"]));

        // taken once: a value already stored, even empty, is never filled again
        var again = await c.Forms.AutoValuesAsync(saved.Fields, new Dictionary<string, string?> { ["requester"] = "ahmed", ["requesterName"] = null, ["department"] = null, ["requestDate"] = "2026-11-09", ["createdBy"] = "ahmed" }, default);
        Assert.Empty(again);
    }

    [Fact]
    public async Task Sections_can_have_a_frame_and_a_background_and_separators_divide_the_form()
    {
        var c = Setup(("LEAVE", Xml("USER", "LEAVE_REQUEST")));
        await using var _db = c.Db;
        WfeFormControl Heading(string text, bool frame, string? colour) => new("heading", null, text, null, null, false, false, null, "full", frame, colour);
        var separator = new WfeFormControl("separator", null, null, null, null, false, false, null, "full");

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => c.Forms.SaveAsync(null,
            Document("LEAVE_REQUEST", LeaveFields, Heading("Leave", true, "purple"), Field("leaveType")), default));
        Assert.Contains("not one of the section colours", string.Join(" | ", bad.Errors["layout"]));

        var saved = await c.Forms.SaveAsync(null, Document("LEAVE_REQUEST", LeaveFields,
            Heading("Leave", true, "Teal"), Field("leaveType"), Field("fromDate"), separator, Field("days"),
            Heading("Notes", false, null), Field("reason"), new WfeFormControl("note", null, "Plain text", null, null, false, false, null, "full", true, "teal")), default);
        var layout = saved.Layout.Controls;
        Assert.Equal((true, "teal"), (layout[0].Frame, layout[0].Background));
        Assert.Equal(("separator", null), (layout[3].Kind, layout[3].Label));
        Assert.Equal((false, null), (layout[5].Frame, layout[5].Background));
        // only a heading carries section settings
        Assert.Equal((false, null), (layout[7].Frame, layout[7].Background));
    }
}
