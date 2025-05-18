namespace talk2me_dotnet_api.Models;

public class GroupMessageModel
{
    public string SenderId { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public string Content { get; set; } = null!;
}