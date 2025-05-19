using Microsoft.EntityFrameworkCore;
using talk2me_dotnet_api.Contexts;
using talk2me_dotnet_api.Dtos;
using talk2me_dotnet_api.Entities;
using talk2me_dotnet_api.Interfaces;

namespace talk2me_dotnet_api.Services;

public class ChatService(AppDbContext context) : IChatService
{
    private readonly AppDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Conversation> GetOrCreateConversationAsync(int userId1, int userId2)
    {
        // Check existing conversation
        var conversation = await _context
            .Conversations.Include(c => c.Participants)
            .FirstOrDefaultAsync(c =>
                c.Type == ConversationType.Private
                && c.Participants.Any(p => p.UserId == userId1)
                && c.Participants.Any(p => p.UserId == userId2)
            );

        if (conversation != null)
            return conversation;

        // Create a new conversation
        conversation = new Conversation
        {
            Type = ConversationType.Private,
            Participants = new List<UserConversation>
            {
                new() { UserId = userId1 },
                new() { UserId = userId2 }
            }
        };

        await _context.Conversations.AddAsync(conversation);
        await _context.SaveChangesAsync();

        return conversation;
    }

    public async Task<Message> SendMessageAsync(int conversationId, int senderId, string content)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Create message
            var message = new Message
            {
                Content = content,
                ConversationId = conversationId,
                SenderId = senderId,
                SentAt = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(message);

            // Update conversation last message
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
                throw new InvalidOperationException("Conversation not found");
            conversation.LastMessageContent = content;
            conversation.LastMessageSenderId = senderId;
            conversation.LastMessageAt = message.SentAt;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return message;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task MarkMessageAsReadAsync(int messageId, int userId)
    {
        var messageRead = new MessageRead
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };

        await _context.MessageReads.AddAsync(messageRead);
        await _context.SaveChangesAsync();
    }

    public async Task<Conversation> CreateGroupConversationAsync(
        int adminId,
        IEnumerable<int> userIds,
        string groupName
    )
    {
        // Combine all participants including admin
        var allParticipants = userIds.ToList();
        if (!allParticipants.Contains(adminId))
        {
            allParticipants.Add(adminId);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create conversation
            var conversation = new Conversation
            {
                Type = ConversationType.Group,
                GroupName = groupName,
                CreatedAt = DateTime.UtcNow,
                Participants = allParticipants
                    .Select(userId => new UserConversation
                    {
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    })
                    .ToList()
            };

            await _context.Conversations.AddAsync(conversation);

            // Create group metadata
            var group = new Group
            {
                ConversationId = conversation.Id,
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return conversation;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ApplicationException("Failed to create group conversation", ex);
        }
    }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(
        int conversationId,
        int limit = 50
    )
    {
        return await _context
            .Messages.Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .Include(m => m.Sender)
            .Include(m => m.ReadBy)
            .ThenInclude(r => r.User)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping)
    {
        var participant = await _context.UserConversations.FirstOrDefaultAsync(uc =>
            uc.ConversationId == conversationId && uc.UserId == userId
        );

        if (participant == null)
        {
            throw new InvalidOperationException("User not part of this conversation");
        }

        participant.IsTyping = isTyping;
        participant.LastTypingActivity = isTyping ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        await _context.Entry(participant).ReloadAsync();
    }

    public async Task<ConversationsResponse> GetUserConversationsAsync(
        int userId,
        int pageNumber,
        int pageSize
    )
    {
        var query = _context
            .UserConversations.Where(uc => uc.UserId == userId)
            .Include(uc => uc.Conversation)
            .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Include(uc => uc.Conversation)
            .ThenInclude(c => c.Group)
            .Include(uc => uc.Conversation)
            .ThenInclude(c => c.Participants)
            .ThenInclude(p => p.User)
            .OrderByDescending(uc => uc.Conversation.LastMessageAt)
            .AsSplitQuery();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var conversations = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(uc => new ConversationDto(
                uc.ConversationId,
                (
                    uc.Conversation.Type == ConversationType.Group
                        ? uc.Conversation.GroupName
                        : uc.Conversation.Participants.First(p => p.UserId != userId).User.Username
                ) ?? string.Empty,
                uc.Conversation.LastMessageContent ?? "No messages",
                uc.Conversation.LastMessageAt,
                uc.Conversation.LastMessageSenderId,
                uc.Conversation.LastMessageSender != null
                    ? uc.Conversation.LastMessageSender.Username
                    : null,
                _context.Messages.Count(m =>
                    m.ConversationId == uc.ConversationId && m.ReadBy.All(r => r.UserId != userId)
                ),
                uc.Conversation.Type,
                uc.Conversation.Type == ConversationType.Group
                    ? uc.Conversation.Participants.Count
                    : null,
                uc.Conversation.Type == ConversationType.Group
                    ? uc.Conversation.Group != null
                        ? uc.Conversation.Group.AvatarUrl
                        : null
                    : uc.Conversation.Participants.First(p => p.UserId != userId).User.AvatarUrl
            ))
            .ToListAsync();

        return new ConversationsResponse(conversations, pageNumber, totalPages, totalItems);
    }
}
