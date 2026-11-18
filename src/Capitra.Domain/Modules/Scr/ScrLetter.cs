using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>SCR_LETTERS: an incoming letter (a court, prosecutor or authority order) that decisions are entered from.</summary>
public class ScrLetter : AuditableEntity
{
    public long LetterId { get; set; }

    /// <summary>Internal number, INT-000001.</summary>
    public string LetterNo { get; set; } = string.Empty;

    /// <summary>The sender's own reference on the letter.</summary>
    public string? SenderReference { get; set; }

    public string SenderName { get; set; } = string.Empty;

    /// <summary>One of: COURIER, EMAIL, HAND, PORTAL.</summary>
    public string ChannelCode { get; set; } = "COURIER";

    public DateOnly LetterDate { get; set; }

    public DateTime ReceivedAt { get; set; }

    /// <summary>One of: RECEIVED, INDEXED (decisions entered from it), CLOSED.</summary>
    public string StatusCode { get; set; } = "RECEIVED";

    public string? Notes { get; set; }

    public List<ScrLetterFile> Files { get; set; } = [];
}

/// <summary>SCR_LETTER_FILES: a scan of the letter (PDF or image), kept with its SHA-256 so it can be shown to be unchanged.</summary>
public class ScrLetterFile : AuditableEntity
{
    public long LetterFileId { get; set; }

    public long LetterId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>SHA-256 of the content, hex.</summary>
    public string Sha256Hash { get; set; } = string.Empty;

    public byte[] FileBlob { get; set; } = [];
}
