using talk2me_dotnet_api.Dtos;
using talk2me_dotnet_api.Entities;

namespace talk2me_dotnet_api.Interfaces;

public interface IChatService
{
    Task<Conversation> GetOrCreateConversationAsync(int userId1, int userId2);
    Task<Conversation> CreateGroupConversationAsync(
        int adminId,
        IEnumerable<int> userIds,
        string groupName
    );
    Task<Message> SendMessageAsync(int conversationId, int senderId, string content);
    Task MarkMessageAsReadAsync(int messageId, int userId);
    Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int limit = 50);
    Task UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping);
    Task<ConversationsResponse> GetUserConversationsAsync(int userId, int pageNumber, int pageSize);
}
