using Capitra.Api.Security;
using Capitra.Api.Startup;
using Capitra.Application;
using Capitra.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCapitraInfrastructure(builder.Configuration);
builder.Services.AddCapitraApplication();
builder.Services.AddCapitraSecurity(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi().AllowAnonymous();
else
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DatabaseStartup.RunAsync(app.Services, app.Configuration);

await app.RunAsync();

/// <summary>Entry point, public for WebApplicationFactory in the tests.</summary>
public partial class Program;
