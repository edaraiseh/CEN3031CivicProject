using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool IsOfficial { get; set; } // default value `false` enforced at database level 
        public DateTime CreatedAt { get; set; } // defualt value `CURRENT_TIMESTAMP` enforced at database level

        public required UserAuth Auth { get; set; }
        public required UserProfile Profile { get; set; }
        public List<Message> Messages { get; set; } = new();
        public List<UserInConversation> Conversations { get; set; } = new();
        public List<Event> CreatedEvents { get; set; } = new();
        public List<EventFollow> FollowedEvents { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
        public List<PostReaction> Reactions { get; set; } = new();
        public List<PetitionSignature> SignedPetitions { get; set; } = new();  
    }
    public class UserAuth
    {
        public int UserId { get; set; }
        public required byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public required byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public DateTime LastLogin { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime LockUntil { get; set; }

        public User? User { get; set; }
    }
    public class UserProfile
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public string? Pic { get; set; }
        public string? Bio { get; set; } 
        public User? User { get; set; }
    }
    public class Post
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public int? ParentId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsOfficial { get; set; }
        public bool IsDeleted { get; set; }

        public User? Author { get; set; }
        public Post? ParentPost { get; set; }
        public List<PostReaction> Reactions { get; set; } = new();
    }
    public enum ReactionType
    {
        Like = 0,
        Dislike,
        Heart
    }
    public class PostReaction
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required int PostId { get; set; }
        public required ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? Author { get; set; }
        public Post? Post { get; set; }
    }
    public enum PetitionStatus
    {
        PendingVerification = 0,
        Open = 1,
        Passed = 2,
        Failed = 3
    }
    public class Petition : Post
    {
        public int SignatureCount { get; set; }
        public required string Title { get; set; }
        public PetitionStatus Status { get; set; }

        public List<PetitionSignature> Signatures { get; set; } = new();  
    }
    public class PetitionSignature
    {
        public required int PetitionId { get; set; }
        public required int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Petition? Petition{ get; set; }
        public User? User { get; set; }
    }
    public class Message
    {
        public int Id { get; set; }
        public required int ConversationId { get; set; }
        public required int SenderId { get; set; }
        public required string Content { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }

        public required Conversation Conversation { get; set; }
        public required User User { get; set; }
    }
    public class Conversation
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<Message> Messages { get; set; } = new();
        public List<UserInConversation> Users { get; set; } = new();
    }
    public class UserInConversation
    {
        public required int UserId { get; set; }
        public required int ConversationId { get; set; }
        public int? LastReadMessageId { get; set; }
        public DateTime JoinedAt { get; set; }

        public required User User { get; set; }
        public required Conversation Conversation { get; set; }
    }
    public enum EventVisibility
    {
        Private = 0,
        FriendsOnly = 1,
        Public = 2
    }
    public class Event
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public required EventVisibility Visibility { get; set; } 

        public required User Author { get; set; }
        public List<EventFollow> Followers { get; set; } = new();
    }
    public class EventFollow
    {
        public required int UserId { get; set; }
        public required int EventId { get; set; }
        public bool IsNotified { get; set; }

        public required User User { get; set; }
        public required Event Event { get; set; }
    }
}