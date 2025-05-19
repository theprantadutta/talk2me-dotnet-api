using Microsoft.AspNetCore.Mvc;
using talk2me_dotnet_api.Interfaces;

namespace talk2me_dotnet_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatService chatService, ILogger<ChatController> logger)
    : ControllerBase
{
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            return Ok(
                await chatService.SendMessageAsync(
                    request.ConversationId,
                    request.SenderId,
                    request.Content
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message");
            return StatusCode(500, "Error sending message");
        }
    }

    [HttpPost("conversations/group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            return Ok(
                await chatService.CreateGroupConversationAsync(
                    request.AdminId,
                    request.MemberIds,
                    request.GroupName
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating group");
            return StatusCode(500, "Error creating group");
        }
    }

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int limit = 50)
    {
        try
        {
            var messages = await chatService.GetConversationMessagesAsync(conversationId, limit);
            return Ok(
                messages.Select(m => new MessageResponse(
                    m.Id,
                    m.Content,
                    m.SenderId,
                    m.SentAt,
                    m.ReadBy.Select(r => r.UserId)
                ))
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving messages");
            return StatusCode(500, "Error retrieving messages");
        }
    }

    [HttpPost("messages/read")]
    public async Task<IActionResult> MarkMessageAsRead([FromBody] MarkReadRequest request)
    {
        try
        {
            await chatService.MarkMessageAsReadAsync(request.MessageId, request.UserId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking message as read");
            return StatusCode(500, "Error marking message as read");
        }
    }

    [HttpPost("typing")]
    public async Task<IActionResult> UpdateTypingStatus([FromBody] TypingStatusRequest request)
    {
        try
        {
            await chatService.UpdateTypingStatusAsync(
                request.ConversationId,
                request.UserId,
                request.IsTyping
            );

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating typing status");
            return StatusCode(500, "Error updating typing status");
        }
    }
}

// Supporting DTOs
public record SendMessageRequest(int ConversationId, int SenderId, string Content);

public record CreateGroupRequest(int AdminId, List<int> MemberIds, string GroupName);

public record MarkReadRequest(int MessageId, int UserId);

public record TypingStatusRequest(int ConversationId, int UserId, bool IsTyping);

public record MessageResponse(
    int Id,
    string Content,
    int SenderId,
    DateTime SentAt,
    IEnumerable<int> ReadBy
);
