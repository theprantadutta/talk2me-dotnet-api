using System.ComponentModel.DataAnnotations;

namespace talk2me_dotnet_api.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string UniqueUserId { get; set; } = null!;

    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [MaxLength(100)]
    public string? Password { get; set; }

    [MaxLength(200)]
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public LoginProvider LoginProvider { get; set; } = null!;

    public ICollection<UserConversation> Conversations { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
