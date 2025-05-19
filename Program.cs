using Serilog;
using talk2me_dotnet_api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);


builder.Services.AddControllers();

builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MqttClientService>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter",
        corsPolicyBuilder => corsPolicyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Add Serilog Request Logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowFlutter");

app.Run();