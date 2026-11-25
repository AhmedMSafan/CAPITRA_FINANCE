using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>The status of a Consumer Finance supplier application.</summary>
public static class CrmCfSupplierStatus
{
    /// <summary>Being entered by Sales.</summary>
    public const string Draft = "DRAFT";

    /// <summary>Submitted: its approval workflow is running.</summary>
    public const string InWorkflow = "IN_WORKFLOW";

    /// <summary>Returned by a workflow step for correction; Sales corrects it and submits again (a new workflow run).</summary>
    public const string Returned = "RETURNED";

    /// <summary>Approved and the contract signed: the supplier role and its agreement are active.</summary>
    public const string Active = "ACTIVE";

    /// <summary>Withdrawn by Sales before approval.</summary>
    public const string Withdrawn = "WITHDRAWN";
}

/// <summary>
/// CRM_CF_SUPPLIER_APPLICATIONS: a Consumer Finance supplier (merchant) application and the agreement terms it asks for (Ahmed,
/// 25 Nov 2026). It goes through CF_SUPPLIER_APPROVAL: document check, compliance (owners and signatories screened, risk rating),
/// site visit, risk review, manager approval (and committee approval when the risk is HIGH), contract signing. When the contract
/// step is accepted the supplier role becomes ACTIVE and the agreement is in force from its start date.
/// </summary>
public class CrmCfSupplierApplication : AuditableEntity
{
    public long ApplicationId { get; set; }

    /// <summary>CFS-000001: the workflow's business reference.</summary>
    public string ApplicationRef { get; set; } = string.Empty;

    public long ClientId { get; set; }

    /// <summary>The customer's SUPPLIER role for CONSUMER_FINANCE.</summary>
    public long ClientRoleId { get; set; }

    public string StatusCode { get; set; } = CrmCfSupplierStatus.Draft;

    /// <summary>The latest workflow run; a returned application starts a new run when submitted again.</summary>
    public long? WorkflowInstanceId { get; set; }

    public string EnteredBy { get; set; } = string.Empty;

    /// <summary>The person who entered it (employee number, or user name): no workflow step may be done by the same person.</summary>
    public string EnteredByPerson { get; set; } = string.Empty;

    public string? ReturnReason { get; set; }

    // ---- agreement terms

    /// <summary>The goods categories the supplier may sell on finance (codes of LND_LKP_GOODS_CATEGORIES, comma-separated).</summary>
    public string GoodsCategoryCodes { get; set; } = string.Empty;

    /// <summary>The least down payment, as a % of the price; a finance takes the higher of this and the product's.</summary>
    public decimal MinDownPaymentPct { get; set; }

    /// <summary>Who takes the down payment from the customer: SUPPLIER (we then pay the price less it) or COMPANY (us).</summary>
    public string DownPaymentCollectorCode { get; set; } = "SUPPLIER";

    /// <summary>What we keep from each payment to the supplier, as a % of the financed price.</summary>
    public decimal CommissionPct { get; set; }

    /// <summary>We pay the supplier this many days after the delivery is confirmed.</summary>
    public int PaymentDays { get; set; }

    /// <summary>How long a supplier quotation holds its price.</summary>
    public int QuotationValidityDays { get; set; }

    /// <summary>How long the supplier has to deliver after our delivery order.</summary>
    public int DeliveryDays { get; set; }

    public DateOnly ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    /// <summary>The customer's bank account (CRM_CLIENT_BANK_ACCOUNTS) the supplier is paid to; it must be VERIFIED to activate.</summary>
    public long? ClientBankAccountId { get; set; }

    // ---- set by the workflow steps

    /// <summary>LOW, MEDIUM or HIGH: the customer's risk grade when the compliance step accepted it; HIGH adds the committee approval.</summary>
    public string? RiskRatingCode { get; set; }

    public DateOnly? SiteVisitDate { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// CRM_CF_SUPPLIER_DOCUMENTS: a file of a supplier application (Ahmed, 25 Nov 2026): one list, added by any Capitra user at any time
/// (before submitting, at any workflow step, or after activation). STEP_CODE records where it was added.
/// </summary>
public class CrmCfSupplierDocument : AuditableEntity
{
    public long DocumentId { get; set; }

    public long ApplicationId { get; set; }

    /// <summary>The workflow run the file belongs to; null for data entry before the first run.</summary>
    public long? WorkflowInstanceId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string StepName { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    /// <summary>
    /// COMMERCIAL_REGISTER, TAX_CARD, ACTIVITY_LICENCE, BANK_LETTER, ID_DOCUMENT, SITE_VISIT_REPORT, SIGNED_AGREEMENT or OTHER. The site
    /// visit step needs a SITE_VISIT_REPORT and the contract step a SIGNED_AGREEMENT; null for a file without a type.
    /// </summary>
    public string? DocumentTypeCode { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>SHA-256 of the content, hex.</summary>
    public string Sha256Hash { get; set; } = string.Empty;

    public byte[] FileBlob { get; set; } = [];

    public bool IsActive { get; set; } = true;
}

/// <summary>CRM_CF_SUPPLIER_STEP_NOTES: what was decided at a completed workflow step of a supplier application, with its comment.</summary>
public class CrmCfSupplierStepNote : AuditableEntity
{
    public long StepNoteId { get; set; }

    public long ApplicationId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public long StepInstanceId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string StepName { get; set; } = string.Empty;

    /// <summary>ACCEPT or REJECT (returned for correction).</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? CommentText { get; set; }

    public string DoneBy { get; set; } = string.Empty;

    public DateTime DoneAt { get; set; }
}
