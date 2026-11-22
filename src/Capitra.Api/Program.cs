using Capitra.Api.Security;
using Capitra.Api.Startup;
using Capitra.Application;
using Capitra.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCapitraInfrastructure(builder.Configuration);
builder.Services.AddCapitraApplication();
// Customer 360: days a bank account waits after its last change before it can be paid (Ahmed, 4 Nov 2026: 2)
builder.Services.AddSingleton(new Capitra.Application.Modules.Crm.CrmSettings(builder.Configuration.GetValue("Crm:BankAccountWaitingDays", 2)));
builder.Services.AddCapitraSecurity(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHostedService<Capitra.Api.Startup.WfeSlaWorker>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// the client's real address behind a web server or proxy on the same machine (IIS, nginx, the dev server); only the local
// machine's forwarded address is trusted, so a remote client cannot fake it (the sign-in log records it)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
});
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi().AllowAnonymous();
else
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<Capitra.Api.Security.PasswordChangeGate>();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

await DatabaseStartup.RunAsync(app.Services, app.Configuration);

await app.RunAsync();

/// <summary>Entry point, public for WebApplicationFactory in the tests.</summary>
public partial class Program;
