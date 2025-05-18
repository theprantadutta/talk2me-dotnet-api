namespace talk2me_dotnet_api.Models;

public class TypingIndicatorModel
{
    public string SenderId { get; set; } = null!;
    public string RecipientId { get; set; } = null!;
    public bool IsTyping { get; set; }
}