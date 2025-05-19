using talk2me_dotnet_api.Entities;

namespace talk2me_dotnet_api.Dtos;

public record ConversationDto(
    int ConversationId,
    string Name,
    string LastMessage,
    DateTime LastMessageAt,
    int? LastMessageSenderId,
    string? LastMessageSenderName,
    int UnreadCount,
    ConversationType Type,
    int? ParticipantCount,
    string? AvatarUrl
);
