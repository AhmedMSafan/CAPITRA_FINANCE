using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Scr;

public sealed class ScrMemberCompanyConfiguration : IEntityTypeConfiguration<ScrMemberCompany>
{
    public void Configure(EntityTypeBuilder<ScrMemberCompany> builder)
    {
        builder.ToTable("SCR_MEMBER_COMPANIES");
        builder.HasKey(x => x.MemberCompanyId);
        builder.HasIndex(x => x.CompanyCode).IsUnique();
        builder.Property(x => x.MemberCompanyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CompanyCode).HasMaxLength(30);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.LicenceNo).HasMaxLength(50);
        builder.Property(x => x.ContactEmail).HasMaxLength(200);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.BillingCycleCode).HasMaxLength(20).HasDefaultValue("MONTHLY");
        builder.HasOne<ScrPricePlan>().WithMany().HasForeignKey(x => x.PricePlanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrLetterConfiguration : IEntityTypeConfiguration<ScrLetter>
{
    public void Configure(EntityTypeBuilder<ScrLetter> builder)
    {
        builder.ToTable("SCR_LETTERS");
        builder.HasKey(x => x.LetterId);
        builder.HasIndex(x => x.LetterNo).IsUnique();
        builder.Property(x => x.LetterId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LetterNo).HasMaxLength(20);
        builder.Property(x => x.SenderReference).HasMaxLength(100);
        builder.Property(x => x.SenderName).HasMaxLength(200);
        builder.Property(x => x.ChannelCode).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasMany(x => x.Files).WithOne().HasForeignKey(x => x.LetterId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class ScrLetterFileConfiguration : IEntityTypeConfiguration<ScrLetterFile>
{
    public void Configure(EntityTypeBuilder<ScrLetterFile> builder)
    {
        builder.ToTable("SCR_LETTER_FILES");
        builder.HasKey(x => x.LetterFileId);
        builder.HasIndex(x => x.LetterId);
        builder.Property(x => x.LetterFileId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FileName).HasMaxLength(260);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.Sha256Hash).HasMaxLength(64);
        builder.ConfigureAudit();
    }
}

public sealed class ScrPartyConfiguration : IEntityTypeConfiguration<ScrParty>
{
    public void Configure(EntityTypeBuilder<ScrParty> builder)
    {
        builder.ToTable("SCR_PARTIES");
        builder.HasKey(x => x.PartyId);
        builder.HasIndex(x => new { x.PartyTypeCode, x.IdentityNo }).IsUnique();
        builder.Property(x => x.PartyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PartyTypeCode).HasMaxLength(20);
        builder.Property(x => x.NameAr).HasMaxLength(300);
        builder.Property(x => x.NameEn).HasMaxLength(300);
        builder.Property(x => x.IdentityTypeCode).HasMaxLength(30);
        builder.Property(x => x.IdentityNo).HasMaxLength(30);
        builder.Property(x => x.NationalityCode).HasMaxLength(2);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasMany(x => x.Aliases).WithOne().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class ScrPartyAliasConfiguration : IEntityTypeConfiguration<ScrPartyAlias>
{
    public void Configure(EntityTypeBuilder<ScrPartyAlias> builder)
    {
        builder.ToTable("SCR_PARTY_ALIASES");
        builder.HasKey(x => x.PartyAliasId);
        builder.HasIndex(x => x.PartyId);
        builder.Property(x => x.PartyAliasId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AliasName).HasMaxLength(300);
        builder.ConfigureAudit();
    }
}

public sealed class ScrDecisionConfiguration : IEntityTypeConfiguration<ScrDecision>
{
    public void Configure(EntityTypeBuilder<ScrDecision> builder)
    {
        builder.ToTable("SCR_DECISIONS");
        builder.HasKey(x => x.DecisionId);
        builder.HasIndex(x => x.DecisionRef).IsUnique();
        builder.HasIndex(x => x.StatusCode);
        builder.HasIndex(x => x.ParentDecisionId);
        builder.HasIndex(x => x.LetterId);
        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.Property(x => x.DecisionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DecisionRef).HasMaxLength(20);
        builder.Property(x => x.OfficialNo).HasMaxLength(100);
        builder.Property(x => x.DecisionTypeCode).HasMaxLength(20);
        builder.Property(x => x.ScopeText).HasMaxLength(2000);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.ReturnReason).HasMaxLength(1000);
        builder.Property(x => x.EnteredBy).HasMaxLength(256);
        builder.Property(x => x.EnteredByPerson).HasMaxLength(300);
        builder.Property(x => x.ReviewedBy).HasMaxLength(256);
        builder.Property(x => x.ReviewedByPerson).HasMaxLength(300);
        builder.Property(x => x.ApprovedBy).HasMaxLength(256);
        builder.Property(x => x.ApprovedByPerson).HasMaxLength(300);
        builder.HasOne<ScrDecision>().WithMany().HasForeignKey(x => x.ParentDecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ScrLetter>().WithMany().HasForeignKey(x => x.LetterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Parties).WithOne().HasForeignKey(x => x.DecisionId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class ScrDecisionPartyConfiguration : IEntityTypeConfiguration<ScrDecisionParty>
{
    public void Configure(EntityTypeBuilder<ScrDecisionParty> builder)
    {
        builder.ToTable("SCR_DECISION_PARTIES");
        builder.HasKey(x => x.DecisionPartyId);
        builder.HasIndex(x => new { x.DecisionId, x.PartyId }).IsUnique();
        builder.HasIndex(x => x.PartyId);
        builder.Property(x => x.DecisionPartyId).UseIdentityByDefaultColumn();
        builder.HasOne<ScrParty>().WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrRegistryVersionConfiguration : IEntityTypeConfiguration<ScrRegistryVersion>
{
    public void Configure(EntityTypeBuilder<ScrRegistryVersion> builder)
    {
        builder.ToTable("SCR_REGISTRY_VERSIONS");
        builder.HasKey(x => x.RegistryVersionId);
        builder.HasIndex(x => x.VersionNo).IsUnique();
        builder.HasIndex(x => x.DecisionId);
        builder.Property(x => x.RegistryVersionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PublishedBy).HasMaxLength(256);
        builder.HasOne<ScrDecision>().WithMany().HasForeignKey(x => x.DecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInquiryConfiguration : IEntityTypeConfiguration<ScrInquiry>
{
    public void Configure(EntityTypeBuilder<ScrInquiry> builder)
    {
        builder.ToTable("SCR_INQUIRIES");
        builder.HasKey(x => x.InquiryId);
        builder.HasIndex(x => x.InquiryRef).IsUnique();
        builder.HasIndex(x => new { x.MemberCompanyId, x.ScreenedAt });
        builder.Property(x => x.InquiryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InquiryRef).HasMaxLength(30);
        builder.Property(x => x.RequestedBy).HasMaxLength(256);
        builder.Property(x => x.ChannelCode).HasMaxLength(20).HasDefaultValue("PORTAL");
        builder.HasOne<ScrApiClient>().WithMany().HasForeignKey(x => x.ApiClientId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.SubjectTypeCode).HasMaxLength(20);
        builder.Property(x => x.SubjectName).HasMaxLength(300);
        builder.Property(x => x.IdentityTypeCode).HasMaxLength(30);
        builder.Property(x => x.IdentityNo).HasMaxLength(30);
        builder.Property(x => x.NationalityCode).HasMaxLength(2);
        builder.Property(x => x.PurposeCode).HasMaxLength(30);
        builder.Property(x => x.PurposeText).HasMaxLength(500);
        builder.Property(x => x.CaseReference).HasMaxLength(100);
        builder.Property(x => x.ResultCode).HasMaxLength(30);
        builder.HasOne<ScrMemberCompany>().WithMany().HasForeignKey(x => x.MemberCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Hits).WithOne().HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.SessionId).HasMaxLength(32);
        builder.HasIndex(x => x.SessionId);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInquiryHitConfiguration : IEntityTypeConfiguration<ScrInquiryHit>
{
    public void Configure(EntityTypeBuilder<ScrInquiryHit> builder)
    {
        builder.ToTable("SCR_INQUIRY_HITS");
        builder.HasKey(x => x.InquiryHitId);
        builder.HasIndex(x => x.InquiryId);
        builder.HasIndex(x => x.PartyId);
        builder.HasIndex(x => x.DecisionId);
        builder.Property(x => x.InquiryHitId).UseIdentityByDefaultColumn();
        builder.Property(x => x.MatchBasisCode).HasMaxLength(20);
        builder.Property(x => x.MatchedName).HasMaxLength(300);
        builder.Property(x => x.EntryStatusCode).HasMaxLength(20);
        builder.HasOne<ScrParty>().WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ScrDecision>().WithMany().HasForeignKey(x => x.DecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrDecisionDocumentConfiguration : IEntityTypeConfiguration<ScrDecisionDocument>
{
    public void Configure(EntityTypeBuilder<ScrDecisionDocument> builder)
    {
        builder.ToTable("SCR_DECISION_DOCUMENTS");
        builder.HasKey(x => x.DocumentId);
        builder.HasIndex(x => new { x.DecisionId, x.StepCode });
        builder.Property(x => x.DocumentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepName).HasMaxLength(200);
        builder.Property(x => x.DocumentTitle).HasMaxLength(300);
        builder.Property(x => x.FileName).HasMaxLength(260);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.Sha256Hash).HasMaxLength(64);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasOne<ScrDecision>().WithMany().HasForeignKey(x => x.DecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrDecisionStepNoteConfiguration : IEntityTypeConfiguration<ScrDecisionStepNote>
{
    public void Configure(EntityTypeBuilder<ScrDecisionStepNote> builder)
    {
        builder.ToTable("SCR_DECISION_STEP_NOTES");
        builder.HasKey(x => x.StepNoteId);
        builder.HasIndex(x => x.DecisionId);
        builder.HasIndex(x => x.StepInstanceId).IsUnique();
        builder.Property(x => x.StepNoteId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepName).HasMaxLength(200);
        builder.Property(x => x.ActionCode).HasMaxLength(20);
        builder.Property(x => x.CommentText).HasMaxLength(2000);
        builder.Property(x => x.DoneBy).HasMaxLength(256);
        builder.HasOne<ScrDecision>().WithMany().HasForeignKey(x => x.DecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrCompanyLicenceConfiguration : IEntityTypeConfiguration<ScrCompanyLicence>
{
    public void Configure(EntityTypeBuilder<ScrCompanyLicence> builder)
    {
        builder.ToTable("SCR_COMPANY_LICENCES");
        builder.HasKey(x => x.LicenceId);
        builder.HasIndex(x => x.MemberCompanyId);
        builder.Property(x => x.LicenceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasOne<ScrMemberCompany>().WithMany().HasForeignKey(x => x.MemberCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrPricePlanConfiguration : IEntityTypeConfiguration<ScrPricePlan>
{
    public void Configure(EntityTypeBuilder<ScrPricePlan> builder)
    {
        builder.ToTable("SCR_PRICE_PLANS");
        builder.HasKey(x => x.PricePlanId);
        builder.HasIndex(x => x.PlanCode).IsUnique();
        builder.Property(x => x.PricePlanId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PlanCode).HasMaxLength(30);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.UserMonthlyFee).HasPrecision(18, 2);
        builder.Property(x => x.InquiryFee).HasPrecision(18, 2);
        builder.Property(x => x.InquiryTiers).HasColumnType("jsonb");
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.ConfigureAudit();
    }
}

public sealed class ScrUserActivityConfiguration : IEntityTypeConfiguration<ScrUserActivity>
{
    public void Configure(EntityTypeBuilder<ScrUserActivity> builder)
    {
        builder.ToTable("SCR_USER_ACTIVITIES");
        builder.HasKey(x => x.UserActivityId);
        builder.HasIndex(x => new { x.MemberCompanyId, x.ActiveFrom });
        builder.HasIndex(x => x.UserId);
        builder.Property(x => x.UserActivityId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.HasOne<ScrMemberCompany>().WithMany().HasForeignKey(x => x.MemberCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInvoiceConfiguration : IEntityTypeConfiguration<ScrInvoice>
{
    public void Configure(EntityTypeBuilder<ScrInvoice> builder)
    {
        builder.ToTable("SCR_INVOICES");
        builder.HasKey(x => x.InvoiceId);
        builder.HasIndex(x => x.InvoiceNo).IsUnique();
        builder.HasIndex(x => new { x.MemberCompanyId, x.PeriodFrom });
        builder.Property(x => x.InvoiceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InvoiceNo).HasMaxLength(30);
        builder.Property(x => x.PlanCode).HasMaxLength(30);
        builder.Property(x => x.UserMonthlyFee).HasPrecision(18, 2);
        builder.Property(x => x.InquiryFee).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.UserAmount).HasPrecision(18, 2);
        builder.Property(x => x.InquiryAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.IssuedBy).HasMaxLength(256);
        builder.Property(x => x.CancelledBy).HasMaxLength(256);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.GlTranSetId).HasMaxLength(50);
        builder.Property(x => x.CancelGlTranSetId).HasMaxLength(50);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.HasOne<ScrMemberCompany>().WithMany().HasForeignKey(x => x.MemberCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ScrPricePlan>().WithMany().HasForeignKey(x => x.PricePlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Bands).WithOne().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInvoiceLineConfiguration : IEntityTypeConfiguration<ScrInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ScrInvoiceLine> builder)
    {
        builder.ToTable("SCR_INVOICE_LINES");
        builder.HasKey(x => x.InvoiceLineId);
        builder.HasIndex(x => new { x.InvoiceId, x.LineNo }).IsUnique();
        builder.Property(x => x.InvoiceLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.FullName).HasMaxLength(256);
        builder.Property(x => x.MonthsText).HasMaxLength(500);
        builder.Property(x => x.UserAmount).HasPrecision(18, 2);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInvoiceBandConfiguration : IEntityTypeConfiguration<ScrInvoiceBand>
{
    public void Configure(EntityTypeBuilder<ScrInvoiceBand> builder)
    {
        builder.ToTable("SCR_INVOICE_BANDS");
        builder.HasKey(x => x.InvoiceBandId);
        builder.HasIndex(x => new { x.InvoiceId, x.BandNo }).IsUnique();
        builder.Property(x => x.InvoiceBandId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BandTypeCode).HasMaxLength(10);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.ConfigureAudit();
    }
}

public sealed class ScrInvoicePaymentConfiguration : IEntityTypeConfiguration<ScrInvoicePayment>
{
    public void Configure(EntityTypeBuilder<ScrInvoicePayment> builder)
    {
        builder.ToTable("SCR_INVOICE_PAYMENTS");
        builder.HasKey(x => x.PaymentId);
        builder.HasIndex(x => x.InvoiceId);
        builder.Property(x => x.PaymentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.GlTranSetId).HasMaxLength(50);
        builder.HasOne<ScrInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Capitra.Domain.Modules.Trs.TrsBankAccount>().WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class ScrApiClientConfiguration : IEntityTypeConfiguration<ScrApiClient>
{
    public void Configure(EntityTypeBuilder<ScrApiClient> builder)
    {
        builder.ToTable("SCR_API_CLIENTS");
        builder.HasKey(x => x.ApiClientId);
        builder.Property(x => x.ApiClientId).UseIdentityByDefaultColumn();
        builder.HasIndex(x => x.ClientId).IsUnique();
        builder.HasIndex(x => x.MemberCompanyId);
        builder.Property(x => x.ClientId).HasMaxLength(60);
        builder.Property(x => x.ClientName).HasMaxLength(150);
        builder.Property(x => x.SecretHash).HasMaxLength(200);
        builder.Property(x => x.SecretHint).HasMaxLength(10);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.RevokedBy).HasMaxLength(256);
        builder.Property(x => x.LastIpAddress).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasOne<ScrMemberCompany>().WithMany().HasForeignKey(x => x.MemberCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
