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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Initializing MqttClientService...");

        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        _logger.LogInformation("MQTT client created");

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com")
            .WithClientId($"dotnet_client_{Guid.NewGuid()}")
            .WithCleanSession()
            .Build();
        _logger.LogInformation("MQTT client options configured: Server={Server}, ClientId={ClientId}", 
            "broker.hivemq.com", _options.ClientId);

        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync;
    }

    public async Task SendUserMessage(string recipientId, string senderId, string content)
    {
        _logger.LogInformation("Sending user message: To={RecipientId}, From={SenderId}", recipientId, senderId);
        await PublishAsync($"user/{recipientId}", content, senderId);
    }

    public async Task SendGroupMessage(string groupId, string senderId, string content)
    {
        _logger.LogInformation("Sending group message: GroupId={GroupId}, From={SenderId}", groupId, senderId);
        await PublishAsync($"group/{groupId}", content, senderId);
    }

    public async Task SendTypingIndicator(string recipientId, string senderId, bool isTyping)
    {
        var topic = $"typing/{recipientId}";
        var message = new 
        {
            SenderId = senderId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Sending typing indicator: To={RecipientId}, From={SenderId}, IsTyping={IsTyping}", 
            recipientId, senderId, isTyping);

        await PublishAsync(topic, message, senderId, "typing");
    }

    public async Task SendGroupTypingIndicator(string groupId, string senderId, bool isTyping)
    {
        var topic = $"group/{groupId}/typing";
        var message = new 
        {
            GroupId = groupId,
            SenderId = senderId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Sending group typing indicator: GroupId={GroupId}, SenderId={SenderId}, IsTyping={IsTyping}", 
            groupId, senderId, isTyping);

        await PublishAsync(topic, message, senderId, "typing");
    }

    private Task HandleConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected successfully");
        return Task.CompletedTask;
    }

    private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogWarning("MQTT client disconnected. Attempting to reconnect in 5 seconds...");
        await Task.Delay(TimeSpan.FromSeconds(5));

        try
        {
            await _mqttClient.ConnectAsync(_options);
            _logger.LogInformation("Reconnected to MQTT broker successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MQTT reconnection attempt failed.");
        }
    }

    private Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var topic = arg.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);

        _logger.LogInformation("Message received: Topic={Topic}, Payload={Payload}", topic, payload);
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT client service...");

        try
        {
            await _mqttClient.ConnectAsync(_options, cancellationToken);
            _logger.LogInformation("MQTT client connected to broker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT client service...");

        await _mqttClient.DisconnectAsync(
            new MqttClientDisconnectOptions { ReasonString = "Shutting down" },
            cancellationToken);

        _logger.LogInformation("MQTT client disconnected.");
    }

    private async Task PublishAsync(string topic, object content, string userId, string clientType = "webapi")
    {
        _logger.LogInformation("Preparing to publish message: Topic={Topic}, UserId={UserId}, ClientType={ClientType}", 
            topic, userId, clientType);

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("MQTT client not connected. Attempting to reconnect...");
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
        _logger.LogDebug("Serialized message: {Json}", json);

        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
        _logger.LogInformation("Published message to topic: {Topic}", topic);
    }
}
