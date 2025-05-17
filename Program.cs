using talk2me_dotnet_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowFlutter");

app.Run();