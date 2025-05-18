namespace talk2me_dotnet_api.Models;

public class UserMessageModel
{
    public string SenderId { get; set; } = null!;
    public string RecipientId { get; set; } = null!;
    public string Content { get; set; } = null!;
}