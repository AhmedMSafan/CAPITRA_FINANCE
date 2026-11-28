using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// A file attached to a decision at one step: data entry (DATA_ENTRY) or a workflow step (SCR_REVIEW, SCR_APPROVAL). Any number per
/// step. Once the step is completed its files are view-only; a file removed while its step was open is kept with IsActive false.
/// </summary>
public class ScrDecisionDocument : AuditableEntity
{
    public long DocumentId { get; set; }

    public long DecisionId { get; set; }

    /// <summary>The workflow run the file belongs to; null for data entry before the first run.</summary>
    public long? WorkflowInstanceId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string StepName { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>SHA-256 of the content, hex.</summary>
    public string Sha256Hash { get; set; } = string.Empty;

    public byte[] FileBlob { get; set; } = [];

    public bool IsActive { get; set; } = true;
}

/// <summary>What was decided at a completed workflow step of a decision, with the comment written there.</summary>
public class ScrDecisionStepNote : AuditableEntity
{
    public long StepNoteId { get; set; }

    public long DecisionId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public long StepInstanceId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string StepName { get; set; } = string.Empty;

    /// <summary>ACCEPT or REJECT.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? CommentText { get; set; }

    public string DoneBy { get; set; } = string.Empty;

    public DateTime DoneAt { get; set; }
}
