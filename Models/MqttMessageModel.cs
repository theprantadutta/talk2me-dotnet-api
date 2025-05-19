namespace talk2me_dotnet_api.Models;

public class MqttMessageModel
{
    public string Topic { get; set; } = null!;
    public string Content { get; set; }= null!;
    public string UserId { get; set; }= null!;  // Add user identifier
    public string ClientType { get; set; } = "webapi";  // To identify backend-originated messages
}