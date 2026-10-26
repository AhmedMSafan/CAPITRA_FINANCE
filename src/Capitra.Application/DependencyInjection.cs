using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Application.Modules.Wfe;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCapitraApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdmLookupService, AdmLookupService>();



        // Portfolio workflow steps: one service per step.

        // General ledger (APEX App 120).
        services.AddScoped<GlCoaSetup>();
        services.AddScoped<GlPeriodSetup>();
        services.AddScoped<GlPeriodControl>();
        services.AddScoped<GlCoaCombination>();
        services.AddScoped<GlJournalProcessing>();
        services.AddScoped<GlPostingEngine>();
        services.AddScoped<GlPostingApi>();

        services.AddScoped<WfeRuntime>();
        services.AddScoped<WfeDefinitions>();
        services.AddScoped<WfeInbox>();

        // Copied APEX pages: every on-demand process found in this assembly, looked up by app, page and name.
        foreach (var type in typeof(DependencyInjection).Assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && typeof(IApexProcess).IsAssignableFrom(t)))
            services.AddScoped(typeof(IApexProcess), type);

        // Wizard-built reports and forms of the copied pages.
        foreach (var region in Modules.Gl.Pages.GlApexRegions.All.Concat(Modules.Mtg.Pages.MtgApexRegions.All))
            services.AddSingleton(region);
        foreach (var lov in Modules.Gl.Pages.GlApexRegions.Lovs.Concat(Modules.Mtg.Pages.MtgApexRegions.Lovs))
            services.AddSingleton(lov);

        // Reports on a SQL query and SQL select lists, translated from Oracle (MtgApexSql), and the page processes around the forms.
        foreach (var report in Modules.Mtg.Pages.MtgApexSql.Reports)
            services.AddSingleton(report);
        foreach (var lov in Modules.Mtg.Pages.MtgApexSql.Lovs)
            services.AddSingleton(lov);
        foreach (var load in Modules.Mtg.Pages.MtgApexSql.Loads)
            services.AddSingleton(load);
        foreach (var type in typeof(DependencyInjection).Assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IApexFormHooks).IsAssignableFrom(t)))
            services.AddScoped(typeof(IApexFormHooks), type);
        services.AddScoped<ApexProcessRegistry>();
        services.AddSingleton<ApexSessionStore>();
        services.AddScoped<ApexSession>();

        // The PL/SQL packages App 105 calls, ported.
        services.AddScoped<Modules.Mtg.Packages.MtgFinancePackage>();
        services.AddScoped<Modules.Mtg.Packages.MtgPurchasedPortfolioPackage>();
        services.AddScoped<Modules.Mtg.Packages.MtgValidationPackage>();
        services.AddScoped<Modules.Mtg.Packages.MtgWorkflowSteps>();

        return services;
    }
}
