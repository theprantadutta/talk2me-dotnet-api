using System.ComponentModel.DataAnnotations;

namespace talk2me_dotnet_api.Entities;

public class Group
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    [MaxLength(50)]
    public string UniqueGroupId { get; set; } = null!;

    public int AdminId { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = null!;

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public ICollection<User> Admin { get; set; } = [];
}
