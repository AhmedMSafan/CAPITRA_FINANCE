using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Application.Modules.Adm.Lookups;
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

        // Copied APEX pages: every on-demand process found in this assembly, looked up by app, page and name.
        foreach (var type in typeof(DependencyInjection).Assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && typeof(IApexProcess).IsAssignableFrom(t)))
            services.AddScoped(typeof(IApexProcess), type);

        // Wizard-built reports and forms of the copied pages.
        foreach (var region in Modules.Gl.Pages.GlApexRegions.All)
            services.AddSingleton(region);
        foreach (var lov in Modules.Gl.Pages.GlApexRegions.Lovs)
            services.AddSingleton(lov);
        services.AddScoped<ApexProcessRegistry>();

        return services;
    }
}
