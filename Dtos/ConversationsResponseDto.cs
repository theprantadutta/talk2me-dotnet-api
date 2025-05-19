namespace talk2me_dotnet_api.Dtos;

public record ConversationsResponse(
    List<ConversationDto> Conversations,
    int CurrentPage,
    int TotalPages,
    int TotalItems
);
