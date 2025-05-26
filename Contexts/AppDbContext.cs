using Microsoft.EntityFrameworkCore;
using talk2me_dotnet_api.Entities;

namespace talk2me_dotnet_api.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
    public DbSet<MessageRead> MessageReads => Set<MessageRead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User-Conversation many-to-many
        modelBuilder.Entity<UserConversation>().HasKey(uc => new { uc.UserId, uc.ConversationId });

        modelBuilder
            .Entity<UserConversation>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder
            .Entity<User>()
            .HasOne(u => u.LoginProvider)
            .WithOne(lp => lp.User)
            .HasForeignKey<LoginProvider>(lp => lp.UserId);

        modelBuilder
            .Entity<UserConversation>()
            .HasOne(uc => uc.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(uc => uc.ConversationId);

        // Message relationships
        modelBuilder
            .Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        modelBuilder
            .Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId);

        // Group relationship
        modelBuilder
            .Entity<Group>()
            .HasOne(g => g.Conversation)
            .WithOne(c => c.Group)
            .HasForeignKey<Group>(g => g.ConversationId);

        // Configure MessageRead composite primary key
        modelBuilder.Entity<MessageRead>().HasKey(mr => new { mr.MessageId, mr.UserId });

        // Configure relationships
        modelBuilder
            .Entity<MessageRead>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.ReadBy)
            .HasForeignKey(mr => mr.MessageId);

        modelBuilder
            .Entity<MessageRead>()
            .HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId);

        // Indexes
        modelBuilder.Entity<Conversation>().HasIndex(c => c.LastMessageAt);

        modelBuilder.Entity<Message>().HasIndex(m => m.SentAt);
    }
}
