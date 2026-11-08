using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

/// <summary>
/// One control of a form, in the designer's order. Kind field shows a data field; heading and note are text between them.
/// </summary>
/// <param name="Control">text, textarea, number, amount, date, yesno, dropdown or user.</param>
/// <param name="DataSource">The drop-down's data source; a Choice field without one offers its own choices.</param>
/// <param name="Width">full or half (half sits beside the next half control on a two-column form).</param>
public sealed record WfeFormControl(
    string Kind, string? Field, string? Label, string? Control, string? DataSource, bool Required, bool ReadOnly, string? Help, string? Width);

public sealed record WfeFormLayout(int Columns, IReadOnlyList<WfeFormControl> Controls);

/// <param name="UsedAt">The steps (and Start) of the workflow that use the form now.</param>
public sealed record WfeFormDto(
    long FormId, string FormCode, string FormName, long WorkflowDefinitionId, string WorkflowCode, string WorkflowName, string? Description,
    bool IsActive, int ControlCount, IReadOnlyList<string> UsedAt, WfeFormLayout Layout, int RowVersion);

public sealed record SaveWfeFormRequest(
    string FormCode, string FormName, long WorkflowDefinitionId, string? Description, bool IsActive, WfeFormLayout? Layout, int RowVersion);

/// <summary>A form ready to show: each control with its label, value and drop-down options.</summary>
public sealed record WfeFormViewDto(
    string FormCode, string FormName, string? Description, int Columns, bool Editable, IReadOnlyList<WfeFormControlViewDto> Controls);

public sealed record WfeFormControlViewDto(
    string Kind, string? Field, string Label, string Control, string? FieldType, bool Required, bool ReadOnly, string? Help, string Width,
    string? Value, IReadOnlyList<WfeOptionDto> Options);

public sealed record WfeTaskFormDto(WfeFormViewDto? Form);

public sealed record WfeStartFormItemDto(string WorkflowCode, string WorkflowName, string? Description, string FormCode, string FormName);

public sealed record WfeStartFromFormRequest(string? BusinessReference, IDictionary<string, string?>? Values);

/// <summary>Where the new instance went: its first task's step and who has it.</summary>
public sealed record WfeStartedDto(long InstanceId, string? NextStep, string? NextAssignee, long? InboxId);

/// <summary>
/// Forms (K2's SmartForms, kept simple): a workflow administrator lays out a form over a workflow's data fields, and a step or
/// the start of the workflow uses it (finova:formCode). A step with its own business screen keeps that screen; a step without one
/// is worked on its form. Forms are additive: a workflow without forms runs exactly as before.
/// </summary>
public sealed partial class WfeForms(ICapitraDbContext db, ICurrentUser currentUser, WfeDataSources sources, WfeRuntime runtime)
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
        return rows.Select(Map).ToList();
    }

    public async Task<WfeFormDto> GetAsync(long id, CancellationToken ct) =>
        Map(await db.Set<WfeForm>().AsNoTracking().Include(x => x.Definition).FirstOrDefaultAsync(x => x.FormId == id, ct)
            ?? throw new NotFoundException("Form not found."));

    /// <summary>The data fields a workflow declares now (its designer copy), for laying out its forms.</summary>
    public async Task<IReadOnlyList<WfeDataField>> FieldsAsync(long workflowDefinitionId, CancellationToken ct)
    {
        var def = await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == workflowDefinitionId, ct)
            ?? throw new NotFoundException("Workflow definition not found.");
        return WfeDataFields.Of(def.BpmnXml);
    }

    /// <summary>The active forms of a workflow, for the designer's step settings.</summary>
    public async Task<IReadOnlyList<WfeFormDto>> OfWorkflowAsync(long workflowDefinitionId, CancellationToken ct)
    {
        var rows = await db.Set<WfeForm>().AsNoTracking().Include(x => x.Definition)
            .Where(x => x.WorkflowDefinitionId == workflowDefinitionId).OrderBy(x => x.FormName).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<WfeFormDto> SaveAsync(long? id, SaveWfeFormRequest r, CancellationToken ct)
    {
        var code = (r.FormCode ?? string.Empty).Trim().ToUpperInvariant();
        var name = (r.FormName ?? string.Empty).Trim();
        var errors = new Dictionary<string, string[]>();
        if (!CodePattern().IsMatch(code))
            errors["formCode"] = ["Use 2 to 50 capital letters, digits or _, starting with a letter."];
        if (name.Length == 0)
            errors["formName"] = ["Give the form a name."];
        var def = await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == r.WorkflowDefinitionId, ct);
        if (def is null)
            errors["workflowDefinitionId"] = ["Choose the workflow the form belongs to."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        var layout = await CleanLayoutAsync(r.Layout ?? new WfeFormLayout(1, []), WfeDataFields.Of(def!.BpmnXml), ct);
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
            if ((row.FormCode != code || row.WorkflowDefinitionId != r.WorkflowDefinitionId) && await UsedAtAsync(row, ct) is { Count: > 0 } used)
                throw new BusinessRuleException($"The workflow uses {row.FormCode} at {string.Join(", ", used)}, so its code and workflow cannot change.");
        }

        row.FormCode = code;
        row.FormName = name;
        row.WorkflowDefinitionId = def.WorkflowDefinitionId;
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
            throw new BusinessRuleException($"The workflow uses {row.FormCode} at {string.Join(", ", used)}. Take it off those steps in the designer first.");
        var inVersions = await db.Set<WfeDefinitionVersion>().AsNoTracking().Where(x => x.WorkflowDefinitionId == row.WorkflowDefinitionId)
            .Select(x => x.BpmnXml).ToListAsync(ct);
        if (inVersions.Any(x => Steps(x, row.FormCode).Count > 0))
            throw new BusinessRuleException($"An earlier version of the workflow uses {row.FormCode}, and its instances may still need it. Switch the form off instead.");
        db.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// A layout with its controls checked: every field is a data field of the workflow (once), its control fits the field's type,
    /// and each drop-down has a data source that exists (or the field's own choices).
    /// </summary>
    private async Task<WfeFormLayout> CleanLayoutAsync(WfeFormLayout layout, IReadOnlyList<WfeDataField> fields, CancellationToken ct)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var controls = new List<WfeFormControl>();
        foreach (var (c, i) in (layout.Controls ?? []).Select((c, i) => (c, i + 1)))
        {
            var kind = (c.Kind ?? "field").Trim().ToLowerInvariant();
            var width = string.Equals(c.Width, "half", StringComparison.OrdinalIgnoreCase) ? "half" : "full";
            if (kind is "heading" or "note")
            {
                if (string.IsNullOrWhiteSpace(c.Label))
                    errors.Add($"Item {i}: write the {kind} text.");
                controls.Add(new WfeFormControl(kind, null, c.Label?.Trim(), null, null, false, false, null, "full"));
                continue;
            }

            var f = fields.FirstOrDefault(x => string.Equals(x.Name, c.Field?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (f is null)
            {
                errors.Add($"Item {i}: \"{c.Field}\" is not a data field of this workflow.");
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

    /// <summary>An active form of this workflow by code, or null (the step then works as it did without a form).</summary>
    public async Task<WfeForm?> FindAsync(long workflowDefinitionId, string? formCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(formCode))
            return null;
        var code = formCode.Trim().ToUpperInvariant();
        return await db.Set<WfeForm>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.FormCode == code && x.WorkflowDefinitionId == workflowDefinitionId && x.IsActive, ct);
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
            if (c.Kind is "heading" or "note")
            {
                list.Add(new WfeFormControlViewDto(c.Kind, null, c.Label ?? string.Empty, c.Kind, null, false, true, null, "full", null, []));
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

            list.Add(new WfeFormControlViewDto("field", f.Name, string.IsNullOrWhiteSpace(c.Label) ? f.Caption : c.Label!, control, f.Type,
                !c.ReadOnly && (c.Required || f.Required), c.ReadOnly || !editable, c.Help ?? f.Description, c.Width ?? "full",
                values.TryGetValue(f.Name, out var v) ? v : null, opts));
        }

        return new WfeFormViewDto(form.FormCode, form.FormName, form.Description, layout.Columns, editable, list);
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
        return result;
    }

    /// <summary>The released workflows that can be started from a form, for Start a Workflow.</summary>
    public async Task<IReadOnlyList<WfeStartFormItemDto>> StartListAsync(CancellationToken ct)
    {
        var defs = await db.Set<WfeWorkflowDefinition>().AsNoTracking()
            .Where(x => x.IsActive && x.DefinitionStatus == "released").OrderBy(x => x.WorkflowName).ToListAsync(ct);
        var forms = await db.Set<WfeForm>().AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
        var list = new List<WfeStartFormItemDto>();
        foreach (var d in defs)
        {
            var code = StartFormCode(d.BpmnXml);
            var form = forms.FirstOrDefault(f => f.WorkflowDefinitionId == d.WorkflowDefinitionId && string.Equals(f.FormCode, code, StringComparison.OrdinalIgnoreCase));
            if (form is not null)
                list.Add(new WfeStartFormItemDto(d.WorkflowCode, d.WorkflowName, d.Description, form.FormCode, form.FormName));
        }

        return list;
    }

    /// <summary>The start form of a workflow, with each field's default value.</summary>
    public async Task<WfeFormViewDto> StartFormAsync(string workflowCode, CancellationToken ct)
    {
        var (def, form) = await StartFormOfAsync(workflowCode, ct);
        var fields = WfeDataFields.Of(def.BpmnXml);
        var defaults = fields.Where(f => f.DefaultValue is not null).ToDictionary(f => f.Name, f => f.DefaultValue, StringComparer.OrdinalIgnoreCase);
        return await ViewAsync(form, fields, defaults, true, ct);
    }

    /// <summary>Starts the workflow with the start form's values, as the signed-in user.</summary>
    public async Task<WfeStartedDto> StartAsync(string workflowCode, WfeStartFromFormRequest r, CancellationToken ct)
    {
        var (def, form) = await StartFormOfAsync(workflowCode, ct);
        var values = await AcceptAsync(form, WfeDataFields.Of(def.BpmnXml), r.Values, ct);
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
        var form = await FindAsync(def.WorkflowDefinitionId, StartFormCode(def.BpmnXml), ct)
            ?? throw new BusinessRuleException($"{def.WorkflowName} has no start form. Its module starts it.");
        return (def, form);
    }

    private static string? StartFormCode(string xml)
    {
        try
        {
            return BpmnModel.StartEvent(xml).FormCode;
        }
        catch (Exception e) when (e is InvalidOperationException or System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>The names of the steps (or Start) of a diagram that use this form.</summary>
    private static List<string> Steps(string xml, string formCode)
    {
        try
        {
            return BpmnModel.Elements(xml).Where(e => string.Equals(e.FormCode, formCode, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Type == "startEvent" ? "Start" : e.Name ?? e.StepCode ?? e.Id).ToList();
        }
        catch (Exception e) when (e is InvalidOperationException or System.Xml.XmlException)
        {
            return [];
        }
    }

    private async Task<List<string>> UsedAtAsync(WfeForm row, CancellationToken ct)
    {
        var xml = await db.Set<WfeWorkflowDefinition>().AsNoTracking().Where(x => x.WorkflowDefinitionId == row.WorkflowDefinitionId)
            .Select(x => x.BpmnXml).FirstOrDefaultAsync(ct);
        return xml is null ? [] : Steps(xml, row.FormCode);
    }

    private static WfeFormDto Map(WfeForm x)
    {
        var layout = ParseLayout(x.LayoutJson);
        return new WfeFormDto(x.FormId, x.FormCode, x.FormName, x.WorkflowDefinitionId, x.Definition?.WorkflowCode ?? string.Empty,
            x.Definition?.WorkflowName ?? string.Empty, x.Description, x.IsActive, layout.Controls.Count(c => c.Kind == "field"),
            x.Definition is null ? [] : Steps(x.Definition.BpmnXml, x.FormCode), layout, x.RowVersion);
    }
}
