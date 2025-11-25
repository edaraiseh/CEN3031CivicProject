using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    using Api.Models;
    public class TVDbContext : DbContext
    {
        public TVDbContext(DbContextOptions<TVDbContext> options) : base(options) {} 
        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> Auth { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<UserInConversation> ConversationMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventFollow> EventFollows { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<Petition> Petitions { get; set; }
        public DbSet<PetitionSignature> PetitionSignatures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUser(modelBuilder);
            ConfigureAuth(modelBuilder);
            ConfigureProfiles(modelBuilder);
            ConfigureConversations(modelBuilder);
            ConfigureConversationMembers(modelBuilder);
            ConfigureMessages(modelBuilder);
            ConfigureEvents(modelBuilder);
            ConfigureEventFollows(modelBuilder);
            ConfigurePosts(modelBuilder);
            ConfigurePostReactions(modelBuilder);
            ConfigurePetitions(modelBuilder);
            ConfigurePetitionSignatures(modelBuilder);
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var name = entity.GetTableName();
                if (name != null)
                {
                    entity.SetTableName(name.ToLower());
                }
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower()); 
                }
            }

        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<User>()
                .Property(u => u.IsOfficial)
                .HasDefaultValue(false);
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
        private void ConfigureAuth(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAuth>()
                .HasKey(a => a.UserId);
            modelBuilder.Entity<UserAuth>()
                .Property(a => a.FailedAttempts)
                .HasDefaultValue(0);
            modelBuilder.Entity<UserAuth>()
                .HasOne(a => a.User)
                .WithOne(u => u.Auth)
                .HasForeignKey<UserAuth>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigureProfiles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>()
                .HasKey(a => a.UserId);
            modelBuilder.Entity<UserProfile>()
                .HasOne(a => a.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigureConversations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Conversation>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
        private void ConfigureConversationMembers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInConversation>()
                .HasKey(uc => new { uc.UserId, uc.ConversationId });
            modelBuilder.Entity<UserInConversation>()
                .HasIndex(uc => new { uc.UserId, uc.ConversationId })
                .IsUnique();
            modelBuilder.Entity<UserInConversation>()
                .Property(cm => cm.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<UserInConversation>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.Conversations)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserInConversation>()
                .HasOne(uc => uc.Conversation)
                .WithMany(u => u.Users)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        private void ConfigureMessages(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Message>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<Message>()
                .Property(m => m.IsDeleted)
                .HasDefaultValue(false);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        private void ConfigureEvents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Author)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigureEventFollows(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventFollow>()
                .HasKey(ef => new { ef.UserId, ef.EventId });
            modelBuilder.Entity<EventFollow>()
                .Property(ef => ef.IsNotified)
                .HasDefaultValue(false);
            modelBuilder.Entity<EventFollow>()
                .HasIndex(ef => new { ef.EventId, ef.UserId })
                .IsUnique();
            modelBuilder.Entity<EventFollow>()
                .HasOne(ef => ef.User)
                .WithMany(u => u.FollowedEvents)
                .HasForeignKey(ef => ef.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EventFollow>()
                .HasOne(ef => ef.Event)
                .WithMany(e => e.Followers)
                .HasForeignKey(ef => ef.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigurePosts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>()
                .UseTptMappingStrategy()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<Post>()
                .Property(p => p.IsOfficial)
                .HasDefaultValue(false);
            modelBuilder.Entity<Post>()
                .Property(p => p.IsDeleted)
                .HasDefaultValue(false);
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigurePostReactions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostReaction>()
                .HasKey(pr => new { pr.Id });
            modelBuilder.Entity<PostReaction>()
                .Property(pr => pr.Type)
                .HasConversion<string>();
            modelBuilder.Entity<PostReaction>()
                .Property(pr => pr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<PostReaction>()
                .HasIndex(pr => new { pr.PostId, pr.UserId, pr.Type })
                .IsUnique();
            modelBuilder.Entity<PostReaction>()
                .HasOne(pr => pr.Author)
                .WithMany(u => u.Reactions)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostReaction>()
                .HasOne(pr => pr.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(pr => pr.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private void ConfigurePetitions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Petition>()
                .Property(p => p.SignatureCount)
                .HasDefaultValue(0);
            modelBuilder.Entity<Petition>()
                .Property(p => p.Status)
                .HasDefaultValue(PetitionStatus.PendingVerification);
        }
        private void ConfigurePetitionSignatures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PetitionSignature>()
                .HasKey(ps => new { ps.UserId, ps.PetitionId } );
            modelBuilder.Entity<PetitionSignature>()
                .Property(pr => pr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<PetitionSignature>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.SignedPetitions)
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PetitionSignature>()
                .HasOne(ps => ps.Petition)
                .WithMany(p => p.Signatures)
                .HasForeignKey(ps => ps.PetitionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
