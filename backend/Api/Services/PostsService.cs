using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Api.ServiceUtils;
using Npgsql;

/*
TODO:
ESSENTIALS: GetAll (maybe not ALL) posts, GetAPost, GetPostsByUser, CreatePost, UpdatePost, DeletePost 
EXTRA: ReactToPost, GetPostReactions, GetRepliesToAPost, CreateReplyToAPost, UpdateReplyToAPost, DeleteReplyToAPost
*/
namespace Api.Services
{
    public class PostsService
    {
        private readonly TVDbContext _context;

        public PostsService(TVDbContext context)
        {
            _context = context;
        }
        public async Task<(ServiceReturnCode, PostQueryDTO?)> GetPostsAsync(
            int page,
            int pageSize,
            string sortBy,
            string sortOrder,
            int? userId,
            string? search,
            bool official
        )
        {
            if (page <= 0 || pageSize <= 0) { return (ServiceReturnCode.InvalidInput, null); }
            var sortProperty = typeof(Post).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance); 
            if (sortProperty == null) { return (ServiceReturnCode.InvalidInput, null); }

            var query = _context.Posts.AsQueryable();
            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId);
            }
            if (search != null)
            {
                query = query.Where(p => p.Content.Contains(search));
            }
            query = sortOrder.ToLower() == "asc"
                ? query.OrderBy(sortBy)
                : query.OrderBy(sortBy + " descending");
            query = query
                .Where(p => !p.IsDeleted)
                .Where(p => p.IsOfficial == official)
                .Where(p => p.ParentId == null)
                .Where(po => !_context.Petitions.Any(pe => pe.Id == po.Id));
            var totalItems = await query.CountAsync();
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Author)
                .ToListAsync();
            return (ServiceReturnCode.Success, new PostQueryDTO
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Posts = posts.Select(p => new PostDTO
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Author = p!.Author!.Username,
                    Reactions = p.Reactions
                }).ToList()
            });
        }
        public async Task<(ServiceReturnCode, CreatePostDTO?)> CreatePostAsync(int userId, string content)
        {
            if (String.IsNullOrEmpty(content)) { return (ServiceReturnCode.InvalidInput,null); }
            var post = new Post
            {
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
            };
            var createdPost = await _context.AddAsync(post);
            if (createdPost == null) { return (ServiceReturnCode.InternalError,null); }
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success,new CreatePostDTO
            {
                Id = post.Id,
                Content = post.Content,
            });
        }
        public async Task<(ServiceReturnCode, CreatePostDTO?)> CreateOfficialPostAsync(int userId, string content)
        {
            if (String.IsNullOrEmpty(content)) { return (ServiceReturnCode.InvalidInput,null); }
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            if (user == null) { return (ServiceReturnCode.InternalError,null); }
            if (!user.IsOfficial) { return (ServiceReturnCode.Unauthorized,null); }
            var post = new Post
            {
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsOfficial = true
            };
            var createdPost = await _context.AddAsync(post);
            if (createdPost == null) { return (ServiceReturnCode.InternalError,null); }
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success,new CreatePostDTO
            {
                Id = post.Id,
                Content = post.Content,
            });
        }
        public async Task<(ServiceReturnCode, DeletePostDTO?)> DeletePostAsync(int userId, int postId)
        {
            var postToDelete = await _context
                .Posts
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (postToDelete == null) { return (ServiceReturnCode.NotFound, null); }
            if (postToDelete.UserId != userId) { return (ServiceReturnCode.Unauthorized, null); }
            if (postToDelete.IsDeleted) { return (ServiceReturnCode.NotFound, null); }
            postToDelete.IsDeleted = true;
            postToDelete.UpdatedAt = DateTime.UtcNow;
            if (postToDelete.ParentId == null) // top level post, delete children
            {
                await RecursivelyDeleteReplies(postToDelete.Id);
            }
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success, new DeletePostDTO
            {
                Id = postToDelete.Id
            });
        }

        public async Task<(ServiceReturnCode, UpdatePostDTO?)> UpdatePostAsync(int userId, int postId, string newContent)
        {
            var postToUpdate = await _context
                .Posts
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (postToUpdate == null) { return (ServiceReturnCode.NotFound, null); }
            if (postToUpdate.UserId != userId) { return (ServiceReturnCode.Unauthorized, null); }
            if (postToUpdate.IsDeleted) { return (ServiceReturnCode.NotFound, null); }
            postToUpdate.UpdatedAt = DateTime.UtcNow;
            postToUpdate.Content = newContent;
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success, new UpdatePostDTO
            {
                Id = postToUpdate.Id,
                NewContent = postToUpdate.Content
            });
        }
        public async Task<(ServiceReturnCode, CreatePostDTO?)> CreateReplyAsync(int userId, int parentId, string content)
        {
            if (String.IsNullOrEmpty(content)) { return (ServiceReturnCode.InvalidInput,null); }
            var parentPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == parentId);
            if (parentPost == null)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            if (parentPost.IsDeleted)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            var post = new Post
            {
                UserId = userId,
                ParentId = parentId, 
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ParentPost = parentPost
            };
            var createdPost = await _context.AddAsync(post);
            if (createdPost == null) { return (ServiceReturnCode.InternalError,null); }
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success, new CreatePostDTO
            {
                Id = post.Id,
                Content = post.Content
            });
        }
        public async Task<(ServiceReturnCode, List<PostDTO>)> GetRepliesAsync(int parentId)
        {
            var replies = await _context.Posts
                .Where(p => p.ParentId == parentId)
                .Select(p => new PostDTO
                {
                    Id = p.Id,
                    Content = p.IsDeleted ? null : p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Author = p!.Author!.Username,
                    Reactions = p.Reactions
                }).ToListAsync();
            return (ServiceReturnCode.Success, replies);
        } 
        public async Task<(ServiceReturnCode, CreateReactionDTO?)> CreateReactionAsync(int userId, int postId, ReactionType type)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            if (post.IsDeleted)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            var reaction = await _context.PostReactions.AddAsync(new PostReaction
            {
                UserId = userId,
                PostId = postId,
                Type = type,
                CreatedAt = DateTime.UtcNow
            });
            if (reaction == null) { return (ServiceReturnCode.InternalError,null); }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == "23505") // code for unique violation
            {
                return (ServiceReturnCode.Conflict, null);
            }
            return (ServiceReturnCode.Success, new CreateReactionDTO
            {
                PostId = postId,
                Type = type
            });
        }
        public async Task<(ServiceReturnCode, CreateReactionDTO?)> DeleteReactionAsync(int userId, int postId, ReactionType type)
        {
            var reaction = await _context.PostReactions
                .Where(pr =>
                    pr.UserId == userId &&
                    pr.PostId == postId &&
                    pr.Type == type)
                .FirstOrDefaultAsync();
            if (reaction == null)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            _context.PostReactions.Remove(reaction);
            await _context.SaveChangesAsync();
            return (ServiceReturnCode.Success, new CreateReactionDTO
            {
                PostId = postId,
                Type = type
            });
        }
        public async Task<(ServiceReturnCode, ReactionAggregateDTO?)> GetReactionAggregatesAsync(int postId)
        {
            var counts = await _context.Posts
            .Where(p => p.Id == postId)
            .Select(p => new ReactionAggregateDTO
            {
                PostId = p.Id,
                Likes = p.Reactions.Count(r => r.Type == ReactionType.Like),
                Dislikes = p.Reactions.Count(r => r.Type == ReactionType.Dislike),
                Hearts = p.Reactions.Count(r => r.Type == ReactionType.Heart)
            })
            .FirstOrDefaultAsync();
            if (counts == null)
            {
                return (ServiceReturnCode.NotFound, null);
            }
            return (ServiceReturnCode.Success, counts);
        }
        // Helpers
        private async Task<bool> RecursivelyDeleteReplies(int parentId)
        {
            await _context.Posts
            .Where(p => p.ParentId == parentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsDeleted, true)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
            );

            var childIds = await _context.Posts
                .Where(p => p.ParentId == parentId)
                .Select(p => p.Id)
                .ToListAsync();
            foreach (var childId in childIds)
            {
                await RecursivelyDeleteReplies(childId);
            }
            return true;
        }
    }
}
