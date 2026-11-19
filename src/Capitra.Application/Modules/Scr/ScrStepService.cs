using System.Security.Cryptography;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Scr;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>A file attached to a decision at a step. CanRemove: the signed-in user added it and its step is still open.</summary>
public sealed record ScrDocumentDto(
    long DocumentId, string StepCode, string StepName, string DocumentTitle, string FileName, string MimeType, long FileSizeBytes,
    string CreatedBy, DateTime CreatedAt, bool CanRemove);

/// <summary>
/// One step of a decision's way to the Final List: data entry, then each workflow step of each run (Round 1, 2... after a return).
/// Status DONE (view-only) or OPEN. Action is ACCEPT or REJECT; Comment what was written at the step.
/// </summary>
public sealed record ScrStepViewDto(
    string Key, int Round, string StepCode, string StepName, string Status, string? Action, string? DoneBy, DateTime? DoneAt, string? Comment,
    long? StepInstanceId, IReadOnlyList<ScrDocumentDto> Documents);

/// <summary>A completed step shown in the lists; it opens the decision with that step view-only.</summary>
public sealed record ScrCompletedStepDto(string Key, string StepCode, string StepName, string? Action, string? DoneBy, DateTime? DoneAt);

public sealed record ScrCompleteStepRequest(string? Action, string? Comment);

public sealed record ScrDocumentFile(string FileName, string MimeType, string Sha256Hash, byte[] Content);

/// <summary>
/// The steps of a decision: documents added at each step (any number), the comment and outcome of each completed workflow step,
/// and every completed step shown view-only. A file can be added only while its step is open: at data entry by the decision's
/// editors (the controller checks SCR.DECISION.EDIT), at a workflow step by whoever holds that step's task.
/// </summary>
public sealed class ScrStepService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeInbox inbox)
{
    public const string DataEntryStep = "DATA_ENTRY";
    public const string DataEntryName = "Data entry";
    public const long MaxFileBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> Mimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg", "image/tiff",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/plain",
    };

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    // ---------------------------------------------------------------- documents

    /// <summary>Adds a file at data entry (the decision is a draft or returned).</summary>
    public async Task<ScrDocumentDto> AddDataEntryDocumentAsync(long decisionId, string? title, string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        var d = await DecisionAsync(decisionId, ct);
        if (!DataEntryOpen(d))
            throw new BusinessRuleException($"Data entry of {d.DecisionRef} is closed: its workflow has been run.");
        return await AddAsync(d, null, DataEntryStep, DataEntryName, title, fileName, mimeType, content, ct);
    }

    /// <summary>Adds a file at the workflow step of the signed-in user's task.</summary>
    public async Task<ScrDocumentDto> AddStepDocumentAsync(long decisionId, long inboxId, string? title, string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        var d = await DecisionAsync(decisionId, ct);
        var step = await OpenStepAsync(d, inboxId, ct);
        return await AddAsync(d, d.WorkflowInstanceId, step.StepCode ?? string.Empty, step.StepName ?? step.StepCode ?? string.Empty, title, fileName, mimeType, content, ct);
    }

    /// <summary>Removes a file its user added while its step is still open (kept, marked inactive).</summary>
    public async Task RemoveDocumentAsync(long documentId, CancellationToken ct)
    {
        var doc = await db.Set<ScrDecisionDocument>().FirstOrDefaultAsync(x => x.DocumentId == documentId && x.IsActive, ct)
            ?? throw new NotFoundException("The document was not found.");
        var d = await DecisionAsync(doc.DecisionId, ct);
        if (!CanRemove(doc, d))
            throw new BusinessRuleException("Only the user who added a document can remove it, and only while its step is open.");
        doc.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<ScrDocumentFile> DocumentFileAsync(long documentId, CancellationToken ct)
    {
        var f = await db.Set<ScrDecisionDocument>().AsNoTracking().Where(x => x.DocumentId == documentId && x.IsActive)
            .Select(x => new ScrDocumentFile(x.FileName, x.MimeType, x.Sha256Hash, x.FileBlob)).FirstOrDefaultAsync(ct);
        return f ?? throw new NotFoundException("The document was not found.");
    }

    // ---------------------------------------------------------------- completing a step

    /// <summary>
    /// Completes the signed-in user's task on a decision (ACCEPT or REJECT) and keeps the step's comment. One transaction: when the
    /// step's checks fail, nothing is kept and the task stays open.
    /// </summary>
    public async Task CompleteAsync(long decisionId, long inboxId, ScrCompleteStepRequest r, CancellationToken ct)
    {
        var action = (r.Action ?? string.Empty).Trim().ToUpperInvariant();
        if (action is not ("ACCEPT" or "REJECT"))
            throw BusinessValidationException.For("action", "Accept or return the decision.");
        var comment = ScrValidation.Clean(r.Comment);
        if (action == "REJECT" && comment is null)
            throw BusinessValidationException.For("comment", "Write why the decision is returned.");
        if (comment is { Length: > 2000 })
            throw BusinessValidationException.For("comment", "The comment is longer than 2000 characters.");

        var d = await DecisionAsync(decisionId, ct);
        var step = await OpenStepAsync(d, inboxId, ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        db.Add(new ScrDecisionStepNote
        {
            DecisionId = d.DecisionId, WorkflowInstanceId = step.WorkflowInstanceId, StepInstanceId = step.StepInstanceId,
            StepCode = step.StepCode ?? string.Empty, StepName = step.StepName ?? step.StepCode ?? string.Empty,
            ActionCode = action, CommentText = comment, DoneBy = Me, DoneAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await inbox.CompleteAsync(inboxId, new CompleteInboxItemRequest(action, comment), ct);
        if (action == "REJECT")
        {
            var back = await db.Set<ScrDecision>().FirstAsync(x => x.DecisionId == decisionId, ct);
            back.ReturnReason = $"{back.ReturnReason} {comment}".Trim();
            if (back.ReturnReason.Length > 1000)
                back.ReturnReason = back.ReturnReason[..1000];
            await db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    // ---------------------------------------------------------------- the steps

    /// <summary>Every step of a decision, in order, with its documents: data entry, then each workflow run's steps.</summary>
    public async Task<IReadOnlyList<ScrStepViewDto>> StepsAsync(long decisionId, CancellationToken ct)
    {
        var d = await DecisionAsync(decisionId, ct, tracking: false);
        return (await StepsAsync([d], ct)).GetValueOrDefault(d.DecisionId) ?? [];
    }

    /// <summary>Per decision, its completed steps (the last completion of each step), for the lists.</summary>
    public async Task<Dictionary<long, IReadOnlyList<ScrCompletedStepDto>>> CompletedStepsAsync(IReadOnlyCollection<ScrDecision> decisions, CancellationToken ct)
    {
        var all = await StepsAsync(decisions, ct, withDocuments: false);
        return all.ToDictionary(x => x.Key, x => (IReadOnlyList<ScrCompletedStepDto>)[.. x.Value.Where(s => s.Status == Done)
            .GroupBy(s => s.StepCode).Select(g => g.Last())
            .Select(s => new ScrCompletedStepDto(s.Key, s.StepCode, s.StepName, s.Action, s.DoneBy, s.DoneAt))]);
    }

    private const string Done = "DONE";
    private const string Open = "OPEN";

    private async Task<Dictionary<long, IReadOnlyList<ScrStepViewDto>>> StepsAsync(IReadOnlyCollection<ScrDecision> decisions, CancellationToken ct,
        bool withDocuments = true)
    {
        var result = new Dictionary<long, IReadOnlyList<ScrStepViewDto>>();
        if (decisions.Count == 0)
            return result;
        var ids = decisions.Select(x => x.DecisionId).ToList();
        var refs = decisions.Select(x => x.DecisionRef).ToList();

        var runs = await (
            from i in db.Set<WfeWorkflowInstance>().AsNoTracking()
            join w in db.Set<WfeWorkflowDefinition>().AsNoTracking() on i.WorkflowDefinitionId equals w.WorkflowDefinitionId
            where w.WorkflowCode == ScrStaffService.WorkflowCode && i.BusinessReference != null && refs.Contains(i.BusinessReference)
            select new { i.WorkflowInstanceId, Reference = i.BusinessReference!, i.StartedAt, i.StartedBy })
            .ToListAsync(ct);
        var runIds = runs.Select(x => x.WorkflowInstanceId).ToList();
        var steps = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => runIds.Contains(s.WorkflowInstanceId) && s.ElementType != "startEvent" && s.ElementType != "endEvent" && s.StepCode != null && s.StepCode != "")
            .Select(s => new { s.WorkflowInstanceId, s.StepInstanceId, s.StepCode, s.StepName, s.StepStatus, s.CompletedAt, s.CompletedBy, s.Outcome })
            .ToListAsync(ct);
        var notes = await db.Set<ScrDecisionStepNote>().AsNoTracking().Where(n => ids.Contains(n.DecisionId)).ToDictionaryAsync(n => n.StepInstanceId, ct);
        var docs = withDocuments
            ? await db.Set<ScrDecisionDocument>().AsNoTracking().Where(x => ids.Contains(x.DecisionId) && x.IsActive).OrderBy(x => x.DocumentId)
                .Select(x => new ScrDecisionDocument
                {
                    DocumentId = x.DocumentId, DecisionId = x.DecisionId, WorkflowInstanceId = x.WorkflowInstanceId, StepCode = x.StepCode,
                    StepName = x.StepName, DocumentTitle = x.DocumentTitle, FileName = x.FileName, MimeType = x.MimeType,
                    FileSizeBytes = x.FileSizeBytes, CreatedBy = x.CreatedBy, CreatedAt = x.CreatedAt, IsActive = true,
                }).ToListAsync(ct)
            : [];

        foreach (var d in decisions)
        {
            var list = new List<ScrStepViewDto>();
            var myRuns = runs.Where(r => r.Reference == d.DecisionRef).OrderBy(r => r.WorkflowInstanceId).ToList();
            var myDocs = docs.Where(x => x.DecisionId == d.DecisionId).ToList();
            // data entry files belong to the data entry before the first run started after them (or to the open one)
            List<ScrDocumentDto> EntryDocs(DateTime? from, DateTime? to) => [.. myDocs
                .Where(x => x.StepCode == DataEntryStep && (from is null || x.CreatedAt > from) && (to is null || x.CreatedAt <= to)).Select(x => ToDto(x, d))];

            DateTime? previous = null;
            var round = 0;
            foreach (var run in myRuns)
            {
                round++;
                list.Add(new ScrStepViewDto($"{DataEntryStep}-{round}", round, DataEntryStep, DataEntryName, Done, "RUN", run.StartedBy, run.StartedAt,
                    null, null, EntryDocs(previous, run.StartedAt)));
                previous = run.StartedAt;
                foreach (var s in steps.Where(s => s.WorkflowInstanceId == run.WorkflowInstanceId).OrderBy(s => s.StepInstanceId))
                {
                    var done = s.StepStatus == WfeRuntime.Completed;
                    var note = notes.GetValueOrDefault(s.StepInstanceId);
                    list.Add(new ScrStepViewDto(
                        $"{s.StepCode}-{round}", round, s.StepCode!, s.StepName ?? s.StepCode!, done ? Done : Open,
                        note?.ActionCode ?? (done ? s.Outcome : null), done ? note?.DoneBy ?? s.CompletedBy : null, done ? note?.DoneAt ?? s.CompletedAt : null,
                        note?.CommentText, s.StepInstanceId,
                        [.. myDocs.Where(x => x.WorkflowInstanceId == run.WorkflowInstanceId && x.StepCode == s.StepCode).Select(x => ToDto(x, d))]));
                }
            }

            if (DataEntryOpen(d))
                list.Add(new ScrStepViewDto($"{DataEntryStep}-{round + 1}", round + 1, DataEntryStep, DataEntryName, Open, null, d.EnteredBy, null,
                    null, null, EntryDocs(previous, null)));

            result[d.DecisionId] = list;
        }

        return result;
    }

    // ---------------------------------------------------------------- helpers

    private static bool DataEntryOpen(ScrDecision d) => d.StatusCode is ScrDecisionStatus.Draft or ScrDecisionStatus.Returned;

    private bool CanRemove(ScrDecisionDocument doc, ScrDecision d)
    {
        if (!string.Equals(doc.CreatedBy, currentUser.UserName, StringComparison.OrdinalIgnoreCase))
            return false;
        return doc.StepCode switch
        {
            DataEntryStep => DataEntryOpen(d),
            ScrDecisionWorkflowHandler.ReviewStep => d.StatusCode == ScrDecisionStatus.InReview && doc.WorkflowInstanceId == d.WorkflowInstanceId,
            ScrDecisionWorkflowHandler.ApprovalStep => d.StatusCode == ScrDecisionStatus.AwaitingApproval && doc.WorkflowInstanceId == d.WorkflowInstanceId,
            _ => false,
        };
    }

    private ScrDocumentDto ToDto(ScrDecisionDocument x, ScrDecision d) => new(
        x.DocumentId, x.StepCode, x.StepName, x.DocumentTitle, x.FileName, x.MimeType, x.FileSizeBytes, x.CreatedBy, x.CreatedAt, CanRemove(x, d));

    private async Task<ScrDecision> DecisionAsync(long id, CancellationToken ct, bool tracking = true)
    {
        var q = tracking ? db.Set<ScrDecision>() : db.Set<ScrDecision>().AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.DecisionId == id, ct) ?? throw new NotFoundException($"Decision {id} was not found.");
    }

    /// <summary>The open workflow step behind the signed-in user's task on this decision.</summary>
    private async Task<WfeStepInstance> OpenStepAsync(ScrDecision d, long inboxId, CancellationToken ct)
    {
        var item = await inbox.GetAsync(inboxId, ct); // throws when the task is not the user's
        if (d.WorkflowInstanceId is null || item.InstanceId != d.WorkflowInstanceId)
            throw new BusinessRuleException($"This task is not on decision {d.DecisionRef}.");
        if (item.Status == WfeRuntime.Completed)
            throw new BusinessRuleException("This step is already completed; its documents are view-only.");
        return await db.Set<WfeStepInstance>().AsNoTracking().FirstAsync(x => x.StepInstanceId == item.StepInstanceId, ct);
    }

    private async Task<ScrDocumentDto> AddAsync(ScrDecision d, long? instanceId, string stepCode, string stepName, string? title,
        string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        if (content.Length == 0 || content.Length > MaxFileBytes)
            throw BusinessValidationException.For("file", "The file is empty or larger than 20 MB.");
        var mime = mimeType.Trim().ToLowerInvariant();
        if (!Mimes.Contains(mime))
            throw BusinessValidationException.For("file", "Attach a PDF, image, Word, Excel or text file.");
        var name = Path.GetFileName(fileName.Trim());
        var doc = new ScrDecisionDocument
        {
            DecisionId = d.DecisionId, WorkflowInstanceId = instanceId, StepCode = stepCode, StepName = stepName,
            DocumentTitle = Truncate(ScrValidation.Clean(title) ?? Path.GetFileNameWithoutExtension(name), 300), FileName = Truncate(name, 260),
            MimeType = mime, FileSizeBytes = content.Length, Sha256Hash = Convert.ToHexStringLower(SHA256.HashData(content)), FileBlob = content,
        };
        db.Add(doc);
        await db.SaveChangesAsync(ct);
        doc.CreatedBy = string.IsNullOrEmpty(doc.CreatedBy) ? Me : doc.CreatedBy;
        return ToDto(doc, d);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
