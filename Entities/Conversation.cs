using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace talk2me_dotnet_api.Entities;

public enum ConversationType
{
    Private,
    Group
}

public class Conversation
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string UniqueConversationId { get; set; } = null!;

    public ConversationType Type { get; set; }

    [MaxLength(50)]
    public string? GroupName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastMessageAt { get; set; }

    [MaxLength(500)]
    public string? LastMessageContent { get; set; }

    public int? LastMessageSenderId { get; set; }

    // Navigation properties
    public User? LastMessageSender { get; set; }
    public ICollection<UserConversation> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public Group? Group { get; set; }
}
