using Microsoft.AspNetCore.Mvc;
using talk2me_dotnet_api.Models;
using talk2me_dotnet_api.Services;

namespace talk2me_dotnet_api.Controllers;

[ApiController]
[Route("api/mqtt")]
public class MqttController(MqttClientService mqttClientService) : ControllerBase
{
    // [HttpPost("send/user")]
    // public async Task<IActionResult> SendUserMessage([FromBody] UserMessageModel model)
    // {
    //     try
    //     {
    //         await mqttClientService.SendUserMessage(
    //             model.RecipientId,
    //             model.SenderId,
    //             model.Content
    //         );
    //
    //         return Ok(new { success = true });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { success = false, error = ex.Message });
    //     }
    // }
    //
    // [HttpPost("send/group")]
    // public async Task<IActionResult> SendGroupMessage([FromBody] GroupMessageModel model)
    // {
    //     try
    //     {
    //         await mqttClientService.SendGroupMessage(model.GroupId, model.SenderId, model.Content);
    //
    //         return Ok(new { success = true });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { success = false, error = ex.Message });
    //     }
    // }
    //
    // [HttpPost("typing/user")]
    // public async Task<IActionResult> SendUserTyping([FromBody] TypingIndicatorModel model)
    // {
    //     try
    //     {
    //         await mqttClientService.SendTypingIndicator(
    //             model.RecipientId,
    //             model.SenderId,
    //             model.IsTyping
    //         );
    //
    //         return Ok(new { success = true });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { success = false, error = ex.Message });
    //     }
    // }
    //
    // [HttpPost("typing/group")]
    // public async Task<IActionResult> SendGroupTyping([FromBody] GroupTypingIndicatorModel model)
    // {
    //     try
    //     {
    //         await mqttClientService.SendGroupTypingIndicator(
    //             model.GroupId,
    //             model.SenderId,
    //             model.IsTyping
    //         );
    //
    //         return Ok(new { success = true });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { success = false, error = ex.Message });
    //     }
    // }
}
