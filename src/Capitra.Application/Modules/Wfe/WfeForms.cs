using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

/// <summary>
/// One control of a form, in the designer's order. Kind field shows a data field; heading and note are text between them;
/// separator is a line across the form (with an optional caption).
/// </summary>
/// <param name="Frame">On a heading: its section (the heading and everything up to the next heading) is drawn in a frame.</param>
/// <param name="Background">On a heading: the section's background colour (teal, orange, grey, green, yellow or sand).</param>
/// <param name="Control">text, textarea, number, amount, date, yesno, dropdown or user.</param>
/// <param name="DataSource">The drop-down's data source; a Choice field without one offers its own choices.</param>
/// <param name="Width">full, half (beside the next half control), left or right (that half of its row) on a two-column form.</param>
public sealed record WfeFormControl(
    string Kind, string? Field, string? Label, string? Control, string? DataSource, bool Required, bool ReadOnly, string? Help, string? Width,
    bool Frame = false, string? Background = null)
{
    /// <summary>The section colours a heading can give its section.</summary>
    public static readonly string[] Backgrounds = ["teal", "orange", "grey", "green", "yellow", "sand"];
}

public sealed record WfeFormLayout(int Columns, IReadOnlyList<WfeFormControl> Controls);

/// <param name="WorkflowDefinitionId">Only on a form made before documents: the workflow whose data fields it shows.</param>
/// <param name="UsedAt">Where workflows use it now: "Workflow: Step", or "Workflow (whole workflow)" for a user-defined workflow.</param>
/// <param name="Fields">The document's fields (an older form: its workflow's data fields).</param>
public sealed record WfeFormDto(
    long FormId, string FormCode, string FormName, long? WorkflowDefinitionId, string? WorkflowCode, string? WorkflowName, string? Description,
    bool IsActive, int ControlCount, IReadOnlyList<string> UsedAt, IReadOnlyList<WfeDataField> Fields, WfeFormLayout Layout, int RowVersion)
{
    /// <summary>A document has its own fields; an older form uses one workflow's data fields.</summary>
    public bool IsDocument => WorkflowDefinitionId is null;
}

/// <param name="WorkflowDefinitionId">Leave empty for a document; an older form keeps its workflow.</param>
/// <param name="Fields">The document's own fields. Sending them makes the form a document (an older form becomes one).</param>
public sealed record SaveWfeFormRequest(
    string FormCode, string FormName, long? WorkflowDefinitionId, string? Description, bool IsActive, WfeFormLayout? Layout, int RowVersion,
    IReadOnlyList<WfeDataField>? Fields = null);

/// <summary>A form ready to show: each control with its label, value and drop-down options.</summary>
/// <param name="IsRequest">The document of a user-defined workflow, as the requester sent it: approvers only read it.</param>
public sealed record WfeFormViewDto(
    string FormCode, string FormName, string? Description, int Columns, bool Editable, IReadOnlyList<WfeFormControlViewDto> Controls,
    bool IsRequest = false);

public sealed record WfeFormControlViewDto(
    string Kind, string? Field, string Label, string Control, string? FieldType, bool Required, bool ReadOnly, string? Help, string Width,
    string? Value, IReadOnlyList<WfeOptionDto> Options, bool Frame = false, string? Background = null);

public sealed record WfeTaskFormDto(WfeFormViewDto? Form);

public sealed record WfeStartFormItemDto(string WorkflowCode, string WorkflowName, string? Description, string FormCode, string FormName);

/// <summary>
/// A request the signed-in user sent, for Start a request: where it is now, and the last decision on it.
/// </summary>
/// <param name="Status">IN_PROGRESS, APPROVED, REJECTED, COMPLETED or FAULTED.</param>
public sealed record WfeMyRequestDto(
    long InstanceId, string WorkflowCode, string WorkflowName, string? BusinessReference, DateTime StartedAt, string Status,
    string? CurrentStep, string? LastAction, string? LastActionBy, DateTime? LastActionAt, string? Comment);

/// <summary>A workflow and its type, for the Document Designer's "used by" and the designer's lists.</summary>
public sealed record WfeWorkflowTypeDto(long WorkflowDefinitionId, string WorkflowCode, string WorkflowName, string Type, string? DocumentCode);

public sealed record WfeStartFromFormRequest(string? BusinessReference, IDictionary<string, string?>? Values);

/// <summary>Where the new instance went: its first task's step and who has it.</summary>
public sealed record WfeStartedDto(long InstanceId, string? NextStep, string? NextAssignee, long? InboxId);

/// <summary>
/// Documents (K2's SmartForms, kept simple). The Document Designer is a function of its own: a document has its own fields
/// (type, required, choices) and a layout, and belongs to no workflow. Workflows link it:
/// - a system-built workflow on any step (finova:formCode): a step without a business screen is worked on it;
/// - a user-defined workflow on the whole workflow (finova:documentCode): the requester fills it in from Start a request and
///   every approval step shows it read-only.
/// Forms made before documents keep working on their workflow's data fields. A workflow without documents runs as before.
/// </summary>
public sealed partial class WfeForms(ICapitraDbContext db, ICurrentUser currentUser, WfeDataSources sources, WfeRuntime runtime, IClock? clock = null)
{
    public static readonly string[] Controls = ["text", "textarea", "number", "amount", "date", "yesno", "dropdown", "user"];

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [GeneratedRegex("^[A-Z][A-Z0-9_]{1,49}$")]
    private static partial Regex CodePattern();

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    /// <summary>The controls a data field type can use, the first being the default.</summary>
    public static string[] ControlsFor(string type) => type switch
    {
        "Number" => ["number", "dropdown"],
        "Decimal" => ["amount"],
        "Boolean" => ["yesno"],
        "Date" => ["date"],
        "Choice" => ["dropdown"],
        "User" => ["user"],
        _ => ["text", "textarea", "dropdown"],
    };

    /// <summary>A stored layout; an empty one when it cannot be read.</summary>
    public static WfeFormLayout ParseLayout(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new WfeFormLayout(1, []);
        try
        {
            var layout = JsonSerializer.Deserialize<WfeFormLayout>(json, Json);
            return layout is null ? new WfeFormLayout(1, []) : layout with { Columns = layout.Columns == 2 ? 2 : 1, Controls = layout.Controls ?? [] };
        }
        catch (JsonException)
        {
            return new WfeFormLayout(1, []);
        }
    }

    public async Task<IReadOnlyList<WfeFormDto>> ListAsync(CancellationToken ct)
    {
        var rows = await db.Set<WfeForm>().AsNoTracking().Include(x => x.Definition).OrderBy(x => x.FormName).ToListAsync(ct);
        var workflows = await WorkflowsAsync(ct);
        return rows.Select(x => Map(x, workflows)).ToList();
    }

    public async Task<WfeFormDto> GetAsync(long id, CancellationToken ct) =>
        Map(await db.Set<WfeForm>().AsNoTracking().Include(x => x.Definition).FirstOrDefaultAsync(x => x.FormId == id, ct)
            ?? throw new NotFoundException("Form not found."), await WorkflowsAsync(ct));

    /// <summary>Every workflow with its type and linked document, for the designers.</summary>
    public async Task<IReadOnlyList<WfeWorkflowTypeDto>> WorkflowTypesAsync(CancellationToken ct) =>
        (await WorkflowsAsync(ct)).Select(w =>
        {
            var p = BpmnModel.Process(w.Xml);
            return new WfeWorkflowTypeDto(w.Id, w.Code, w.Name, p.IsUserDefined ? BpmnProcessInfo.User : BpmnProcessInfo.System, p.DocumentCode);
        }).ToList();

    /// <summary>The fields a form shows: a document's own, or (an older form) its workflow's data fields.</summary>
    public static IReadOnlyList<WfeDataField> FieldsOf(WfeForm form, string? workflowXml) =>
        form.FieldsJson is not null ? WfeDataFields.Parse(form.FieldsJson) : workflowXml is null ? [] : WfeDataFields.Of(workflowXml);

    /// <summary>
    /// The workflow's data fields, then the fields of the documents it links (on the workflow or on its steps), for typing the
    /// values an instance stores. A data field of the workflow wins over a document field of the same name.
    /// </summary>
    public static async Task<IReadOnlyList<WfeDataField>> AllFieldsAsync(ICapitraDbContext db, string xml, CancellationToken ct)
    {
        var fields = WfeDataFields.Of(xml).ToList();
        var codes = LinkedCodes(xml);
        if (codes.Count == 0)
            return fields;
        var docs = await db.Set<WfeForm>().AsNoTracking().Where(x => codes.Contains(x.FormCode) && x.FieldsJson != null)
            .Select(x => x.FieldsJson).ToListAsync(ct);
        foreach (var f in docs.SelectMany(WfeDataFields.Parse))
            if (!fields.Exists(x => string.Equals(x.Name, f.Name, StringComparison.OrdinalIgnoreCase)))
                fields.Add(f);
        return fields;
    }

    /// <summary>The document codes a diagram links: the workflow's document and every step's.</summary>
    private static List<string> LinkedCodes(string xml)
    {
        var codes = new List<string>();
        if (BpmnModel.Process(xml).DocumentCode is { } doc)
            codes.Add(doc);
        try
        {
            codes.AddRange(BpmnModel.Elements(xml).Select(e => e.FormCode?.Trim().ToUpperInvariant()).OfType<string>().Where(c => c.Length > 0));
        }
        catch (Exception e) when (e is InvalidOperationException or System.Xml.XmlException)
        {
            // an unreadable diagram links nothing
        }

        return codes.Distinct().ToList();
    }

    /// <summary>The data fields a workflow declares now (its designer copy), for laying out its forms.</summary>
    public async Task<IReadOnlyList<WfeDataField>> FieldsAsync(long workflowDefinitionId, CancellationToken ct)
    {
        var def = await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == workflowDefinitionId, ct)
            ?? throw new NotFoundException("Workflow definition not found.");
        return WfeDataFields.Of(def.BpmnXml);
    }

    /// <summary>The documents a workflow can link (every document, and its own older forms), for the designer.</summary>
    public async Task<IReadOnlyList<WfeFormDto>> OfWorkflowAsync(long workflowDefinitionId, CancellationToken ct)
    {
        var rows = await db.Set<WfeForm>().AsNoTracking().Include(x => x.Definition)
            .Where(x => x.WorkflowDefinitionId == null || x.WorkflowDefinitionId == workflowDefinitionId).OrderBy(x => x.FormName).ToListAsync(ct);
        var workflows = await WorkflowsAsync(ct);
        return rows.Select(x => Map(x, workflows)).ToList();
    }

    public async Task<WfeFormDto> SaveAsync(long? id, SaveWfeFormRequest r, CancellationToken ct)
    {
        var code = (r.FormCode ?? string.Empty).Trim().ToUpperInvariant();
        var name = (r.FormName ?? string.Empty).Trim();
        var errors = new Dictionary<string, string[]>();
        if (!CodePattern().IsMatch(code))
            errors["formCode"] = ["Use 2 to 50 capital letters, digits or _, starting with a letter."];
        if (name.Length == 0)
            errors["formName"] = ["Give the document a name."];
        // a document has its own fields; an older form (no fields sent) keeps its workflow's data fields
        var document = r.Fields is not null || r.WorkflowDefinitionId is null;
        WfeWorkflowDefinition? def = null;
        IReadOnlyList<WfeDataField> fields = [];
        if (document)
        {
            var problems = new List<string>();
            fields = WfeDataFields.Clean(r.Fields, problems);
            if (fields.Count == 0 && problems.Count == 0)
                problems.Add("Add the document's fields.");
            if (problems.Count > 0)
                errors["fields"] = problems.Distinct().ToArray();
        }
        else
        {
            def = await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == r.WorkflowDefinitionId, ct);
            if (def is null)
                errors["workflowDefinitionId"] = ["Choose the workflow the form belongs to."];
            else
                fields = WfeDataFields.Of(def.BpmnXml);
        }

        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        var layout = await CleanLayoutAsync(r.Layout ?? new WfeFormLayout(1, []), fields, document ? "document" : "workflow", ct);
        if (await db.Set<WfeForm>().AnyAsync(x => x.FormCode == code && x.FormId != (id ?? 0), ct))
            throw new ConflictException($"A form with code {code} already exists.");

        WfeForm row;
        if (id is null)
        {
            row = new WfeForm();
            db.Add(row);
        }
        else
        {
            row = await db.Set<WfeForm>().FirstOrDefaultAsync(x => x.FormId == id, ct) ?? throw new NotFoundException("Form not found.");
            if (row.RowVersion != r.RowVersion)
                throw new ConflictException("The form was changed by someone else. Reload it and try again.");
            if (row.FormCode != code && await UsedAtAsync(row, ct) is { Count: > 0 } used)
                throw new BusinessRuleException($"{row.FormCode} is used at {string.Join(", ", used)}, so its code cannot change.");
            if (!document && row.WorkflowDefinitionId != def!.WorkflowDefinitionId && await UsedAtAsync(row, ct) is { Count: > 0 } usedBy)
                throw new BusinessRuleException($"{row.FormCode} is used at {string.Join(", ", usedBy)}, so its workflow cannot change.");
        }

        row.FormCode = code;
        row.FormName = name;
        row.WorkflowDefinitionId = def?.WorkflowDefinitionId;
        row.FieldsJson = document ? WfeDataFields.ToJson(fields) : null;
        row.Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim();
        row.IsActive = r.IsActive;
        row.LayoutJson = JsonSerializer.Serialize(layout, Json);
        await db.SaveChangesAsync(ct);
        return await GetAsync(row.FormId, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var row = await db.Set<WfeForm>().FirstOrDefaultAsync(x => x.FormId == id, ct) ?? throw new NotFoundException("Form not found.");
        if (await UsedAtAsync(row, ct) is { Count: > 0 } used)
            throw new BusinessRuleException($"{row.FormCode} is used at {string.Join(", ", used)}. Take it off there in the workflow designer first.");
        var versions = db.Set<WfeDefinitionVersion>().AsNoTracking();
        if (row.WorkflowDefinitionId is { } wf)
            versions = versions.Where(x => x.WorkflowDefinitionId == wf);
        var inVersions = await versions.Select(x => x.BpmnXml).ToListAsync(ct);
        if (inVersions.Any(x => Steps(x, row.FormCode).Count > 0))
            throw new BusinessRuleException($"An earlier version of a workflow uses {row.FormCode}, and its instances may still need it. Switch it off instead.");
        db.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// A layout with its controls checked: every field is a data field of the workflow (once), its control fits the field's type,
    /// and each drop-down has a data source that exists (or the field's own choices).
    /// </summary>
    private async Task<WfeFormLayout> CleanLayoutAsync(WfeFormLayout layout, IReadOnlyList<WfeDataField> fields, string owner, CancellationToken ct)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var controls = new List<WfeFormControl>();
        foreach (var (c, i) in (layout.Controls ?? []).Select((c, i) => (c, i + 1)))
        {
            var kind = (c.Kind ?? "field").Trim().ToLowerInvariant();
            var width = c.Width?.Trim().ToLowerInvariant() is "half" or "left" or "right" ? c.Width.Trim().ToLowerInvariant() : "full";
            if (kind is "heading" or "note")
            {
                if (string.IsNullOrWhiteSpace(c.Label))
                    errors.Add($"Item {i}: write the {kind} text.");
                var colour = c.Background?.Trim().ToLowerInvariant();
                if (kind == "heading" && !string.IsNullOrEmpty(colour) && !WfeFormControl.Backgrounds.Contains(colour))
                    errors.Add($"Item {i}: \"{c.Background}\" is not one of the section colours.");
                controls.Add(new WfeFormControl(kind, null, c.Label?.Trim(), null, null, false, false, null, "full",
                    kind == "heading" && c.Frame, kind == "heading" && colour is not null && WfeFormControl.Backgrounds.Contains(colour) ? colour : null));
                continue;
            }

            if (kind == "separator")
            {
                controls.Add(new WfeFormControl(kind, null, string.IsNullOrWhiteSpace(c.Label) ? null : c.Label.Trim(), null, null, false, false, null, "full"));
                continue;
            }

            var f = fields.FirstOrDefault(x => string.Equals(x.Name, c.Field?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (f is null)
            {
                errors.Add($"Item {i}: \"{c.Field}\" is not a data field of this {owner}.");
                continue;
            }

            if (!seen.Add(f.Name))
                errors.Add($"\"{f.Caption}\" is on the form twice.");
            var allowed = ControlsFor(f.Type);
            var control = string.IsNullOrWhiteSpace(c.Control) ? allowed[0] : c.Control.Trim().ToLowerInvariant();
            if (!allowed.Contains(control))
                errors.Add($"\"{f.Caption}\" is a {f.Type} field; it can use {string.Join(" or ", allowed)}, not {control}.");
            var source = string.IsNullOrWhiteSpace(c.DataSource) ? null : c.DataSource.Trim().ToUpperInvariant();
            if (control == "user")
                source = null;
            else if (control != "dropdown")
                source = null;
            else if (source is null && f.Choices.Count == 0)
                errors.Add($"\"{f.Caption}\": choose the data source of the drop-down.");
            else if (source is not null && !await sources.ExistsAsync(source, ct))
                errors.Add($"\"{f.Caption}\": data source {source} does not exist.");

            controls.Add(new WfeFormControl("field", f.Name, string.IsNullOrWhiteSpace(c.Label) ? null : c.Label.Trim(), control, source,
                c.Required && !c.ReadOnly, c.ReadOnly, string.IsNullOrWhiteSpace(c.Help) ? null : c.Help.Trim(), width));
        }

        if (errors.Count > 0)
            throw new BusinessValidationException(new Dictionary<string, string[]> { ["layout"] = errors.Distinct().ToArray() });
        return new WfeFormLayout(layout.Columns == 2 ? 2 : 1, controls);
    }

    // ---------- running forms ----------

    /// <summary>An active document (or older form of this workflow) by code, or null (the step then works as without one).</summary>
    public async Task<WfeForm?> FindAsync(long workflowDefinitionId, string? formCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(formCode))
            return null;
        var code = formCode.Trim().ToUpperInvariant();
        return await db.Set<WfeForm>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.FormCode == code && (x.WorkflowDefinitionId == null || x.WorkflowDefinitionId == workflowDefinitionId) && x.IsActive, ct);
    }

    /// <summary>The form with its values and options. Controls of fields the version does not declare are left out.</summary>
    public async Task<WfeFormViewDto> ViewAsync(
        WfeForm form, IReadOnlyList<WfeDataField> fields, IReadOnlyDictionary<string, string?> values, bool editable, CancellationToken ct)
    {
        var layout = ParseLayout(form.LayoutJson);
        var options = new Dictionary<string, IReadOnlyList<WfeOptionDto>>(StringComparer.OrdinalIgnoreCase);
        var list = new List<WfeFormControlViewDto>();
        foreach (var c in layout.Controls)
        {
            if (c.Kind is "heading" or "note" or "separator")
            {
                list.Add(new WfeFormControlViewDto(c.Kind, null, c.Label ?? string.Empty, c.Kind, null, false, true, null, "full", null, [], c.Frame, c.Background));
                continue;
            }

            var f = fields.FirstOrDefault(x => string.Equals(x.Name, c.Field, StringComparison.OrdinalIgnoreCase));
            if (f is null)
                continue;
            var control = c.Control ?? ControlsFor(f.Type)[0];
            IReadOnlyList<WfeOptionDto> opts = [];
            var source = control == "user" ? WfeDataSources.Users : c.DataSource;
            if (source is not null)
            {
                if (!options.TryGetValue(source, out var o))
                    options[source] = o = await sources.OptionsAsync(source, ct);
                opts = o;
            }
            else if (control == "dropdown")
                opts = f.Choices.Select(x => new WfeOptionDto(x, x)).ToList();

            // a field the system fills in (current user, today) is never typed
            var locked = c.ReadOnly || f.Auto is not null;
            list.Add(new WfeFormControlViewDto("field", f.Name, string.IsNullOrWhiteSpace(c.Label) ? f.Caption : c.Label!, control, f.Type,
                !locked && (c.Required || f.Required), locked || !editable, c.Help ?? f.Description ?? (f.Auto is null ? null : $"Filled in automatically: {WfeDataFields.AutoText(f.Auto)}."), c.Width ?? "full",
                values.TryGetValue(f.Name, out var v) ? v : null, opts));
        }

        return new WfeFormViewDto(form.FormCode, form.FormName, form.Description, layout.Columns, editable, list);
    }

    /// <summary>
    /// The values of the fields the system fills in for the signed-in user (user name, full name, department, employee no., today).
    /// With <paramref name="existing"/>, only fields the instance has not stored yet: the value is taken once and never changed.
    /// </summary>
    public async Task<Dictionary<string, string?>> AutoValuesAsync(
        IReadOnlyList<WfeDataField> fields, IReadOnlyDictionary<string, string?>? existing, CancellationToken ct)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        // set once, when the document is first filled: a field already stored (even empty) is never filled again
        var auto = fields.Where(f => f.Auto is not null && (existing is null || !existing.ContainsKey(f.Name))).ToList();
        if (auto.Count == 0)
            return result;
        var me = currentUser.UserName;
        var user = auto.Any(f => f.Auto is not (WfeDataFields.CurrentUser or WfeDataFields.Today)) ? await sources.UserAsync(me, ct) : null;
        var today = clock is null ? DateOnly.FromDateTime(DateTime.UtcNow) : Gl.GlTime.Today(clock);
        foreach (var f in auto)
            result[f.Name] = f.Auto switch
            {
                WfeDataFields.CurrentUser => me,
                WfeDataFields.CurrentUserName => user?.FullName ?? me,
                WfeDataFields.CurrentDepartment => user?.Department,
                WfeDataFields.CurrentEmployeeNo => user?.EmployeeNo,
                _ => today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            };
        return result;
    }

    /// <summary>
    /// The values a form sends, kept to its editable fields: each required one filled, each drop-down value one of its options.
    /// Types are checked afterwards by the runtime (WfeDataFields).
    /// </summary>
    public async Task<Dictionary<string, string?>> AcceptAsync(
        WfeForm form, IReadOnlyList<WfeDataField> fields, IDictionary<string, string?>? given, CancellationToken ct)
    {
        var view = await ViewAsync(form, fields, new Dictionary<string, string?>(), true, ct);
        var input = new Dictionary<string, string?>(given ?? new Dictionary<string, string?>(), StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var errors = new Dictionary<string, string[]>();
        foreach (var c in view.Controls.Where(c => c.Kind == "field" && !c.ReadOnly))
        {
            input.TryGetValue(c.Field!, out var raw);
            var value = string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
            if (value is null)
            {
                if (c.Required)
                    errors[c.Field!] = [$"Fill in \"{c.Label}\"."];
                if (input.ContainsKey(c.Field!))
                    result[c.Field!] = null;
                continue;
            }

            if (c.Control is "dropdown" or "user" && c.Options.Count > 0)
            {
                var match = c.Options.FirstOrDefault(o => string.Equals(o.Value, value, StringComparison.OrdinalIgnoreCase));
                if (match is null)
                {
                    errors[c.Field!] = [$"\"{c.Label}\" must be one of the listed values."];
                    continue;
                }

                value = match.Value;
            }

            result[c.Field!] = value;
        }

        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        // each value in its stored form (1500.25, 2026-11-08, true), checked against its field's type
        return WfeDataFields.ForUpdate(fields, result);
    }

    /// <summary>
    /// The released user-defined workflows a user can start, each on its document, for Start a request. System-built workflows are
    /// started by their modules and are never offered (a workflow drawn before types is offered when its Start has a form).
    /// </summary>
    public async Task<IReadOnlyList<WfeStartFormItemDto>> StartListAsync(CancellationToken ct)
    {
        var defs = await db.Set<WfeWorkflowDefinition>().AsNoTracking()
            .Where(x => x.IsActive && x.DefinitionStatus == "released").OrderBy(x => x.WorkflowName).ToListAsync(ct);
        var forms = await db.Set<WfeForm>().AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
        var list = new List<WfeStartFormItemDto>();
        foreach (var d in defs)
        {
            var code = StartDocumentCode(d.BpmnXml);
            var form = forms.FirstOrDefault(f => (f.WorkflowDefinitionId == null || f.WorkflowDefinitionId == d.WorkflowDefinitionId)
                && string.Equals(f.FormCode, code, StringComparison.OrdinalIgnoreCase));
            if (form is not null)
                list.Add(new WfeStartFormItemDto(d.WorkflowCode, d.WorkflowName, d.Description, form.FormCode, form.FormName));
        }

        return list;
    }

    /// <summary>The requests the signed-in user sent (workflows started on a document), newest first.</summary>
    public async Task<IReadOnlyList<WfeMyRequestDto>> MyRequestsAsync(CancellationToken ct)
    {
        var me = Me;
        var rows = await db.Set<WfeWorkflowInstance>().AsNoTracking()
            .Include(x => x.Definition).Include(x => x.Version).Include(x => x.Steps).Include(x => x.Variables)
            .Where(x => x.StartedBy == me).OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.WorkflowInstanceId).Take(100).ToListAsync(ct);
        var list = new List<WfeMyRequestDto>();
        foreach (var x in rows)
        {
            if (x.Definition is null || StartDocumentCode(WfeVersions.XmlOf(x)) is null)
                continue;
            var decided = x.Steps.Where(s => s.CompletedAt is not null && !string.IsNullOrWhiteSpace(s.Outcome))
                .OrderByDescending(s => s.CompletedAt).ThenByDescending(s => s.StepInstanceId).FirstOrDefault();
            var open = x.Steps.Where(s => s.CompletedAt is null).OrderByDescending(s => s.EnteredAt).FirstOrDefault();
            var outcome = decided?.Outcome?.Trim().ToUpperInvariant();
            var status = x.InstanceStatus.ToUpperInvariant() switch
            {
                "COMPLETED" => outcome is "REJECT" or "REJECTED" ? "REJECTED" : outcome is "APPROVE" or "APPROVED" ? "APPROVED" : "COMPLETED",
                "FAULTED" => "FAULTED",
                _ => "IN_PROGRESS",
            };
            var comment = x.Variables.FirstOrDefault(v => v.VariableName == "TASK_COMMENT")?.VariableValue;
            list.Add(new WfeMyRequestDto(x.WorkflowInstanceId, x.Definition.WorkflowCode, x.Definition.WorkflowName, x.BusinessReference, x.StartedAt,
                status, status == "IN_PROGRESS" ? open?.StepName ?? open?.StepCode : null, decided?.Outcome, decided?.CompletedBy, decided?.CompletedAt,
                comment));
        }

        return list;
    }

    /// <summary>The start form of a workflow, with each field's default value.</summary>
    public async Task<WfeFormViewDto> StartFormAsync(string workflowCode, CancellationToken ct)
    {
        var (def, form) = await StartFormOfAsync(workflowCode, ct);
        var fields = FieldsOf(form, def.BpmnXml);
        var defaults = fields.Where(f => f.DefaultValue is not null).ToDictionary(f => f.Name, f => f.DefaultValue, StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in await AutoValuesAsync(fields, null, ct))
            defaults[name] = value;
        return await ViewAsync(form, fields, defaults, true, ct);
    }

    /// <summary>Starts the workflow with the start form's values, as the signed-in user.</summary>
    public async Task<WfeStartedDto> StartAsync(string workflowCode, WfeStartFromFormRequest r, CancellationToken ct)
    {
        var (def, form) = await StartFormOfAsync(workflowCode, ct);
        var fields = FieldsOf(form, def.BpmnXml);
        var values = await AcceptAsync(form, fields, r.Values, ct);
        // the automatic values come from the server, never from what the browser sent
        foreach (var (name, value) in await AutoValuesAsync(fields, null, ct))
            values[name] = value;
        var reference = string.IsNullOrWhiteSpace(r.BusinessReference) ? null : r.BusinessReference.Trim();
        var instance = await runtime.StartAsync(def.WorkflowDefinitionId, reference, values, Me, ct);
        var next = await db.Set<WfeInboxItem>().AsNoTracking().Include(x => x.StepInstance)
            .Where(x => x.WorkflowInstanceId == instance.WorkflowInstanceId && x.InboxStatus == WfeRuntime.Open)
            .OrderBy(x => x.InboxItemId).FirstOrDefaultAsync(ct);
        return new WfeStartedDto(instance.WorkflowInstanceId, next?.StepInstance?.StepName ?? next?.Subject,
            next is null ? null : next.AssignedRole ?? next.AssignedTo, next?.InboxItemId);
    }

    private async Task<(WfeWorkflowDefinition Def, WfeForm Form)> StartFormOfAsync(string workflowCode, CancellationToken ct)
    {
        var code = workflowCode.Trim().ToUpperInvariant();
        var def = await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowCode == code, ct)
            ?? throw new NotFoundException($"Workflow {code} not found.");
        if (!def.IsActive || !string.Equals(def.DefinitionStatus, "released", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"{def.WorkflowName} is not released, so it cannot be started.");
        if (BpmnModel.Process(def.BpmnXml).IsSystemBuilt)
            throw new BusinessRuleException($"{def.WorkflowName} is a system-built workflow. Its module starts it.");
        var form = await FindAsync(def.WorkflowDefinitionId, StartDocumentCode(def.BpmnXml), ct)
            ?? throw new BusinessRuleException($"{def.WorkflowName} has no document to start it on. Link one in the workflow designer.");
        return (def, form);
    }

    /// <summary>
    /// The document a user starts the workflow on: a user-defined workflow's document; else the Start's form (workflows drawn before
    /// types). Null for a system-built workflow.
    /// </summary>
    public static string? StartDocumentCode(string xml)
    {
        var p = BpmnModel.Process(xml);
        if (p.IsSystemBuilt)
            return null;
        if (p.DocumentCode is not null)
            return p.DocumentCode;
        try
        {
            return BpmnModel.StartEvent(xml).FormCode;
        }
        catch (Exception e) when (e is InvalidOperationException or System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>Where a diagram uses this document: "whole workflow" for a user-defined workflow's document, else its steps (or Start).</summary>
    private static List<string> Steps(string xml, string formCode)
    {
        var list = new List<string>();
        if (string.Equals(BpmnModel.Process(xml).DocumentCode, formCode, StringComparison.OrdinalIgnoreCase))
            list.Add("whole workflow");
        try
        {
            list.AddRange(BpmnModel.Elements(xml).Where(e => string.Equals(e.FormCode, formCode, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Type == "startEvent" ? "Start" : e.Name ?? e.StepCode ?? e.Id));
        }
        catch (Exception e) when (e is InvalidOperationException or System.Xml.XmlException)
        {
            // an unreadable diagram uses nothing
        }

        return list;
    }

    private sealed record Workflow(long Id, string Code, string Name, string Xml);

    private async Task<List<Workflow>> WorkflowsAsync(CancellationToken ct) =>
        await db.Set<WfeWorkflowDefinition>().AsNoTracking().OrderBy(x => x.WorkflowName)
            .Select(x => new Workflow(x.WorkflowDefinitionId, x.WorkflowCode, x.WorkflowName, x.BpmnXml)).ToListAsync(ct);

    /// <summary>
    /// Where workflows use the form now. An older form: its workflow's steps. A document: "Workflow: Step" in every workflow.
    /// </summary>
    private static List<string> UsedAt(WfeForm row, IReadOnlyList<Workflow> workflows)
    {
        if (row.WorkflowDefinitionId is { } wf && row.FieldsJson is null)
            return workflows.Where(w => w.Id == wf).SelectMany(w => Steps(w.Xml, row.FormCode)).ToList();
        return workflows.SelectMany(w => Steps(w.Xml, row.FormCode).Select(s => s == "whole workflow" ? $"{w.Name} (whole workflow)" : $"{w.Name}: {s}")).ToList();
    }

    private async Task<List<string>> UsedAtAsync(WfeForm row, CancellationToken ct) => UsedAt(row, await WorkflowsAsync(ct));

    private static WfeFormDto Map(WfeForm x, IReadOnlyList<Workflow> workflows)
    {
        var layout = ParseLayout(x.LayoutJson);
        return new WfeFormDto(x.FormId, x.FormCode, x.FormName, x.WorkflowDefinitionId, x.Definition?.WorkflowCode,
            x.Definition?.WorkflowName, x.Description, x.IsActive, layout.Controls.Count(c => c.Kind == "field"),
            UsedAt(x, workflows), FieldsOf(x, x.Definition?.BpmnXml), layout, x.RowVersion);
    }
}
