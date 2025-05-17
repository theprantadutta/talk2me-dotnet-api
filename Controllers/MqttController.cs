using Microsoft.AspNetCore.Mvc;
using talk2me_dotnet_api.Services;

namespace talk2me_dotnet_api.Controllers;

[ApiController]
[Route("api/mqtt")]
public class MqttController(MqttClientService mqttClientService) : ControllerBase
{
    [HttpPost("publish")]
    public async Task<IActionResult> PublishMessage([FromBody] MqttMessageModel model)
    {
        if (string.IsNullOrEmpty(model.UserId))
        {
            return BadRequest("UserId is required");
        }

        try
        {
            await mqttClientService.PublishAsync(
                model.Topic, 
                model.Content, 
                model.UserId,
                model.ClientType);
        
            return Ok(new { 
                success = true,
                message = "Message published",
                userId = model.UserId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                success = false,
                error = ex.Message
            });
        }
    }
    
    // You can add more endpoints here to get received messages if needed
}

public class MqttMessageModel
{
    public string Topic { get; set; } = null!;
    public string Content { get; set; }= null!;
    public string UserId { get; set; }= null!;  // Add user identifier
    public string ClientType { get; set; } = "webapi";  // To identify backend-originated messages
}