using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Application.Modules.Mtg.PortfolioSteps.AmlReview;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ChequeCoverage;
using Capitra.Application.Modules.Mtg.PortfolioSteps.CoverageReview;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Inquiries;
using Capitra.Application.Modules.Mtg.PortfolioSteps.LegalReview;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Valuation;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCapitraApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdmLookupService, AdmLookupService>();

        services.AddScoped<IMtgDeveloperService, MtgDeveloperService>();
        services.AddScoped<IMtgDeveloperSignatoryService, MtgDeveloperSignatoryService>();
        services.AddScoped<IMtgProjectService, MtgProjectService>();
        services.AddScoped<IMtgProjectUnitService, MtgProjectUnitService>();
        services.AddScoped<IMtgInvestorService, MtgInvestorService>();

        services.AddScoped<IMtgPortfolioService, MtgPortfolioService>();
        services.AddScoped<IMtgPortfolioAccountService, MtgPortfolioAccountService>();
        services.AddScoped<IMtgAccountPaymentService, MtgAccountPaymentService>();
        services.AddScoped<IMtgPortfolioQueryService, MtgPortfolioQueryService>();

        // Portfolio workflow steps: one service per step.
        services.AddScoped<IMtgExcelImportService, MtgExcelImportService>();
        services.AddScoped<IMtgAmlReviewService, MtgAmlReviewService>();
        services.AddScoped<IMtgInquiryService, MtgInquiryService>();
        services.AddScoped<IMtgLegalReviewService, MtgLegalReviewService>();
        services.AddScoped<IMtgValuationService, MtgValuationService>();
        services.AddScoped<IMtgCoverageReviewService, MtgCoverageReviewService>();
        services.AddScoped<IMtgChequeCoverageService, MtgChequeCoverageService>();
        return services;
    }
}
