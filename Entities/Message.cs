using System.ComponentModel.DataAnnotations;

namespace talk2me_dotnet_api.Entities;

public class Message
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string UniqueMessageId { get; set; } = null!;

    [MaxLength(500)]
    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; } = MessageType.Text;

    // Foreign keys
    public int ConversationId { get; set; }
    public int SenderId { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public ICollection<MessageRead> ReadBy { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    System
}
