using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;
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
        return services;
    }
}
