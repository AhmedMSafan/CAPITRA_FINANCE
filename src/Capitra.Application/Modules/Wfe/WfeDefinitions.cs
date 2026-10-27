using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

public sealed record WfeDefinitionDto(
    long WorkflowDefinitionId,
    string WorkflowCode,
    string WorkflowName,
    string VersionNo,
    string DefinitionStatus,
    string BpmnXml,
    string? Description,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt,
    int RowVersion);

public sealed record SaveWfeDefinitionRequest(
    string WorkflowCode,
    string WorkflowName,
    string? VersionNo,
    string? DefinitionStatus,
    string BpmnXml,
    string? Description,
    bool IsActive,
    int RowVersion);

public sealed record WfeRunningStepDto(string ElementId, string? StepName, string StepStatus, string? Outcome, DateTime EnteredAt, DateTime? CompletedAt, string? CompletedBy);

public sealed record WfeRunningInstanceDto(
    long InstanceId,
    long WorkflowDefinitionId,
    string WorkflowName,
    string VersionNo,
    string? BusinessReference,
    string InstanceStatus,
    string? CurrentElementId,
    DateTime StartedAt,
    string StartedBy,
    DateTime LastActivity,
    IReadOnlyList<WfeRunningStepDto> Steps);

/// <summary>The workflow definitions the BPMN designer reads and saves.</summary>
public sealed class WfeDefinitions(ICapitraDbContext db)
{
    public async Task<IReadOnlyList<WfeDefinitionDto>> ListAsync(string? search, CancellationToken cancellationToken)
    {
        var query = db.Set<WfeWorkflowDefinition>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
#pragma warning disable CA1862 // EF Core translates ToLower to SQL lower()
            query = query.Where(x => x.WorkflowCode.ToLower().Contains(s) || x.WorkflowName.ToLower().Contains(s));
#pragma warning restore CA1862
        }

        return await query.OrderBy(x => x.WorkflowCode).Select(x => Map(x)).ToListAsync(cancellationToken);
    }

    /// <summary>The instances still running (not completed, rejected or cancelled), most recently active first, with their step history.</summary>
    public async Task<IReadOnlyList<WfeRunningInstanceDto>> RunningAsync(CancellationToken cancellationToken)
    {
        string[] closed = ["COMPLETED", "REJECTED", "CANCELLED"];
        var rows = await db.Set<WfeWorkflowInstance>().AsNoTracking()
            .Include(x => x.Definition)
            .Include(x => x.Steps)
            .Where(x => !closed.Contains(x.InstanceStatus.ToUpper()))
            .ToListAsync(cancellationToken);
        return rows
            .Select(x => new WfeRunningInstanceDto(
                x.WorkflowInstanceId, x.WorkflowDefinitionId, x.Definition?.WorkflowName ?? x.WorkflowDefinitionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                x.Definition?.VersionNo ?? "1", x.BusinessReference, x.InstanceStatus, x.CurrentElementId, x.StartedAt, x.StartedBy,
                x.UpdatedAt ?? x.CreatedAt,
                x.Steps.OrderBy(s => s.EnteredAt).ThenBy(s => s.StepInstanceId)
                    .Select(s => new WfeRunningStepDto(s.ElementId, s.StepName, s.StepStatus, s.Outcome, s.EnteredAt, s.CompletedAt, s.CompletedBy)).ToList()))
            .OrderByDescending(x => x.LastActivity).ThenByDescending(x => x.InstanceId)
            .ToList();
    }

    public async Task<WfeDefinitionDto> GetAsync(long id, CancellationToken cancellationToken) =>
        Map(await db.Set<WfeWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == id, cancellationToken)
            ?? throw new NotFoundException("Workflow definition not found."));

    public async Task<WfeDefinitionDto> SaveAsync(long? id, SaveWfeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var code = (request.WorkflowCode ?? string.Empty).Trim().ToUpperInvariant();
        var name = (request.WorkflowName ?? string.Empty).Trim();
        var errors = new Dictionary<string, string[]>();
        if (code.Length == 0)
            errors["workflowCode"] = ["Workflow code is required."];
        if (name.Length == 0)
            errors["workflowName"] = ["Workflow name is required."];
        if (string.IsNullOrWhiteSpace(request.BpmnXml))
            errors["bpmnXml"] = ["BPMN XML is required."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        if (await db.Set<WfeWorkflowDefinition>().AnyAsync(x => x.WorkflowCode == code && x.WorkflowDefinitionId != (id ?? 0), cancellationToken))
            throw new ConflictException($"Workflow code '{code}' already exists.");

        WfeWorkflowDefinition row;
        if (id is null)
        {
            row = new WfeWorkflowDefinition();
            db.Add(row);
        }
        else
        {
            row = await db.Set<WfeWorkflowDefinition>().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == id, cancellationToken)
                ?? throw new NotFoundException("Workflow definition not found.");
            if (row.RowVersion != request.RowVersion)
                throw new ConflictException("The workflow was changed by someone else. Reload it and try again.");
        }

        row.WorkflowCode = code;
        row.WorkflowName = name;
        row.VersionNo = string.IsNullOrWhiteSpace(request.VersionNo) ? "1" : request.VersionNo.Trim();
        row.DefinitionStatus = string.IsNullOrWhiteSpace(request.DefinitionStatus) ? "draft" : request.DefinitionStatus.Trim().ToLowerInvariant();
        row.BpmnXml = request.BpmnXml;
        row.Description = request.Description;
        row.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return Map(row);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var row = await db.Set<WfeWorkflowDefinition>().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == id, cancellationToken)
            ?? throw new NotFoundException("Workflow definition not found.");
        db.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static WfeDefinitionDto Map(WfeWorkflowDefinition x) => new(
        x.WorkflowDefinitionId, x.WorkflowCode, x.WorkflowName, x.VersionNo, x.DefinitionStatus, x.BpmnXml, x.Description, x.IsActive,
        x.CreatedBy, x.CreatedAt, x.UpdatedBy, x.UpdatedAt, x.RowVersion);
}
