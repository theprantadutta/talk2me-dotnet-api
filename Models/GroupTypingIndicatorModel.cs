namespace talk2me_dotnet_api.Models;

public class GroupTypingIndicatorModel
{
    public string SenderId { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public bool IsTyping { get; set; }
}