using System.Collections.Concurrent;
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
    
    // Track active subscriptions
    private readonly List<string> _subscribedTopics = [];
    
    // // Track typing status (userId -> isTyping)
    // private readonly ConcurrentDictionary<string, bool> _typingStatus = new();
    //
    // // Track group memberships (groupId -> List<userId>)
    // private readonly ConcurrentDictionary<string, List<string>> _groups = new();

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
    
    // public async Task SubscribeToUserTopic(string userId)
    // {
    //     var topic = $"user/{userId}";
    //     await SubscribeToTopic(topic);
    // }
    //
    // public async Task SubscribeToGroupTopic(string groupId)
    // {
    //     var topic = $"group/{groupId}";
    //     await SubscribeToTopic(topic);
    // }
    //
    // public async Task SubscribeToTypingTopic(string userId)
    // {
    //     var topic = $"typing/{userId}";
    //     await SubscribeToTopic(topic);
    // }
    //
    // public async Task SubscribeToGroupTypingTopic(string groupId)
    // {
    //     var topic = $"group/{groupId}/typing";
    //     await SubscribeToTopic(topic);
    // }

    private async Task SubscribeToTopic(string topic)
    {
        if (_subscribedTopics.Contains(topic)) return;
        
        await _mqttClient.SubscribeAsync(topic);
        _subscribedTopics.Add(topic);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);
    }

    public async Task SendUserMessage(string recipientId, string senderId, string content)
    {
        var topic = $"user/{recipientId}";
        await PublishAsync(topic, content, senderId);
    }

    public async Task SendGroupMessage(string groupId, string senderId, string content)
    {
        var topic = $"group/{groupId}";
        await PublishAsync(topic, content, senderId);
    }

    public async Task SendTypingIndicator(string recipientId, string senderId, bool isTyping)
    {
        var topic = $"typing/{recipientId}"; // Separate typing topic
        var message = new 
        {
            SenderId = senderId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        };
    
        // Console.WriteLine($"SenderId {senderId} Typing Status: {isTyping}");
        await PublishAsync(topic, JsonSerializer.Serialize(message), senderId, "typing");
    }

    public async Task SendGroupTypingIndicator(string groupId, string senderId, bool isTyping)
    {
        var topic = $"group/{groupId}/typing"; // Separate group typing topic
        var message = new 
        {
            SenderId = senderId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        };
    
        Console.WriteLine($"GroupID {groupId}, SenderId {senderId} Typing Status: {isTyping}");
        await PublishAsync(topic, JsonSerializer.Serialize(message), senderId, "typing");
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

    private async Task PublishAsync(string topic, string content, string userId, string clientType = "webapi")
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