using Capitra.Domain.Common;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

/// <summary>Portfolio case tables of the portfolio purchasing module. Lengths follow the APEX form items.</summary>
public sealed class MtgPurchasedPortfolioConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolio>
{
    public const int CommentsMaxLength = 4000;

    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolio> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIOS");
        builder.HasKey(x => x.PortfolioId);
        builder.HasIndex(x => x.PortfolioNo).IsUnique();
        builder.HasIndex(x => x.DeveloperId);
        builder.HasIndex(x => x.PortfolioStatusCode);

        builder.Property(x => x.PortfolioNo).HasMaxLength(30);
        builder.Property(x => x.PortfolioStatusCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Comments).HasMaxLength(CommentsMaxLength);
        builder.Property(x => x.RejectionReason).HasMaxLength(CommentsMaxLength);
        builder.Property(x => x.BoardDecisionCode).HasMaxLength(20);
        builder.Property(x => x.BoardDecisionNo).HasMaxLength(200);

        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpPortfolioStatus>().WithMany().HasForeignKey(x => x.PortfolioStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgPurchasedPortfolioAccountConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioAccount>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioAccount> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_ACCOUNTS");
        builder.HasKey(x => x.PortfolioAccountId);
        builder.HasIndex(x => new { x.PortfolioId, x.SellerContractNo }).IsUnique();

        builder.Property(x => x.SellerContractNo).HasMaxLength(100);
        builder.Property(x => x.InstallmentFrequency).HasMaxLength(30);
        builder.Property(x => x.AccountStatus).HasMaxLength(30);
        builder.Property(x => x.DealDecisionCode).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgInvestor>().WithMany().HasForeignKey(x => x.InvestorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProjectUnit>().WithMany().HasForeignKey(x => x.ProjectUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgPurchasedPortfolioImportConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioImport>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioImport> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_IMPORTS");
        builder.HasKey(x => x.PortfolioImportId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();

        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.ImportStatusCode).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgAmlResultConfiguration : IEntityTypeConfiguration<MtgAmlResult>
{
    public void Configure(EntityTypeBuilder<MtgAmlResult> builder)
    {
        builder.ToTable("MTG_AML_RESULTS");
        builder.HasKey(x => x.AmlResultId);
        builder.HasIndex(x => new { x.PortfolioId, x.SubjectTypeCode, x.SubjectId });

        builder.Property(x => x.SubjectTypeCode).HasMaxLength(30);
        builder.Property(x => x.AmlTypeCode).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.AmlResultCode).HasMaxLength(50);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(30);
        builder.Property(x => x.ResultDetails).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgInquiryResultConfiguration : IEntityTypeConfiguration<MtgInquiryResult>
{
    public void Configure(EntityTypeBuilder<MtgInquiryResult> builder)
    {
        builder.ToTable("MTG_INQUIRY_RESULTS");
        builder.HasKey(x => x.InquiryResultId);
        builder.HasIndex(x => new { x.PortfolioId, x.SubjectTypeCode, x.SubjectId });

        builder.Property(x => x.SubjectTypeCode).HasMaxLength(30);
        builder.Property(x => x.InquiryTypeCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.ResultText).HasMaxLength(100);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(30);
        builder.Property(x => x.ResultDetails).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpInquiryType>().WithMany().HasForeignKey(x => x.InquiryTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgLegalReviewConfiguration : IEntityTypeConfiguration<MtgLegalReview>
{
    public void Configure(EntityTypeBuilder<MtgLegalReview> builder)
    {
        builder.ToTable("MTG_LEGAL_REVIEWS");
        builder.HasKey(x => x.LegalReviewId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();
        builder.Ignore(x => x.IsComplete);

        builder.Property(x => x.LegalFindings).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);
        builder.Property(x => x.LegalRecommendation).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);
        builder.Property(x => x.ReviewedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgValuationConfiguration : IEntityTypeConfiguration<MtgValuation>
{
    public void Configure(EntityTypeBuilder<MtgValuation> builder)
    {
        builder.ToTable("MTG_VALUATIONS");
        builder.HasKey(x => x.ValuationId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.ValuationTypeCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ValuerName).HasMaxLength(200);
        builder.Property(x => x.ValuerRegistrationNo).HasMaxLength(100);
        builder.Property(x => x.FacilitiesStatus).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProjectUnit>().WithMany().HasForeignKey(x => x.ProjectUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpValuationType>().WithMany().HasForeignKey(x => x.ValuationTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgPurchasedPortfolioChequeConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioCheque>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioCheque> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_CHEQUES");
        builder.HasKey(x => x.ChequeId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);
        builder.Property(x => x.ChequeStatusCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ReturnReason).HasMaxLength(1000);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolioAccount>().WithMany().HasForeignKey(x => x.PortfolioAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpChequeStatus>().WithMany().HasForeignKey(x => x.ChequeStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgPurchasedPortfolioFinancialReviewConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioFinancialReview>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioFinancialReview> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS");
        builder.HasKey(x => x.FinancialReviewId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();

        builder.Property(x => x.PvMethodCode).HasMaxLength(30);
        builder.Property(x => x.MarginBaseCode).HasMaxLength(30);
        builder.Property(x => x.AdminBaseCode).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgAccountPaymentConfiguration : IEntityTypeConfiguration<MtgAccountPayment>
{
    public void Configure(EntityTypeBuilder<MtgAccountPayment> builder)
    {
        builder.ToTable("MTG_ACCOUNT_PAYMENTS");
        builder.HasKey(x => x.AccountPaymentId);

        builder.Property(x => x.PaymentStatusCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);

        builder.HasOne<MtgPurchasedPortfolioAccount>().WithMany().HasForeignKey(x => x.PortfolioAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpPaymentStatus>().WithMany().HasForeignKey(x => x.PaymentStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class MtgPurchasedPortfolioStatusHistoryConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioStatusHistory>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioStatusHistory> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_STATUS_HIST");
        builder.HasKey(x => x.PortfolioStatusHistoryId);

        builder.Property(x => x.FromStatusCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ToStatusCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.Comments).HasMaxLength(MtgPurchasedPortfolioConfiguration.CommentsMaxLength);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
