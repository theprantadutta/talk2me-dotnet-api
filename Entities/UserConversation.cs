namespace talk2me_dotnet_api.Entities;

public class UserConversation
{
    public int UserId { get; set; }
    
    public int ConversationId { get; set; }
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsTyping { get; set; }
    public DateTime? LastTypingActivity { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}