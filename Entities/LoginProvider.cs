using System.ComponentModel.DataAnnotations;

namespace talk2me_dotnet_api.Entities;

public class LoginProvider
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string Provider { get; set; } = null!;
    
    [MaxLength(100)]
    public string ProviderId { get; set; } = null!;
    
    [MaxLength(50)]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}