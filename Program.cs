using Asp.Versioning;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using talk2me_dotnet_api.Contexts;
using talk2me_dotnet_api.Environments;
using talk2me_dotnet_api.Interfaces;
using talk2me_dotnet_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Load .env files environment variables
Env.Load();

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

// Configure Environment variables
builder
    .Configuration
    // only loads variables starting with TALK2ME__
    .AddEnvironmentVariables(s => s.Prefix = "TALK2ME__")
    .Build();

var pgsqlSection = builder.Configuration.GetSection("PGSQL");
builder.Services.Configure<PgsqlConfiguration>(pgsqlSection);

// Add Postgres SQL Db Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgsqlSection.GetValue<string>("CONNECTION_STRING"))
);

// Adding API Versioning
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MqttClientService>());

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFlutter",
        corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

var app = builder.Build();

// Add Serilog Request Logging
app.UseSerilogRequestLogging();

// Mapping health checks url to '/health'
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowFlutter");

app.Run();
