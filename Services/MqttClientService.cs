using System.Text.Json;

namespace talk2me_dotnet_api.Services;

using MQTTnet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MqttClientService : IHostedService
{
    private readonly ILogger<MqttClientService> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;

    public MqttClientService(ILogger<MqttClientService> logger)
    {
        _logger = logger;
        
        // Create MQTT client
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        
        // Configure options
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com") // Public broker for testing
            .WithClientId($"dotnet_client_{Guid.NewGuid()}")
            .WithCleanSession()
            .Build();
            
        // Setup event handlers
        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync;
    }

    private async Task HandleConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected");
        
        // Subscribe to a topic
        await _mqttClient.SubscribeAsync("flutter/demo");
    }

    private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client disconnected");
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        try
        {
            await _mqttClient.ConnectAsync(_options);
        }
        catch
        {
            _logger.LogError("Reconnection failed");
        }
    }

    private Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        _logger.LogInformation("Received message on topic {ApplicationMessageTopic}", arg.ApplicationMessage.Topic);
        var payload = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
        _logger.LogInformation("Message: {Payload}", payload);
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT client");
        try
        {
            await _mqttClient.ConnectAsync(_options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT broker");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT client");
        await _mqttClient.DisconnectAsync(
            new MqttClientDisconnectOptions { ReasonString = "Shutting down" },
            cancellationToken);
    }
    
    public async Task PublishAsync(string topic, string content, string userId, string clientType = "webapi")
    {
        if (!_mqttClient.IsConnected)
        {
            await _mqttClient.ConnectAsync(_options);
        }

        var message = new 
        {
            UserId = userId,
            ClientType = clientType,
            MessageId = Guid.NewGuid().ToString(),
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);
    
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
}