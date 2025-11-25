using System.Text.Json.Serialization;

namespace Api.Models
{
    public class PostQueryDTO
    {
        public required int TotalItems { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
        public required int TotalPages { get; set; }
        public required List<PostDTO> Posts { get; set; }
    }
    public class PostDTO
    {
        public int Id { get; set; }
        public required string Author { get; set; }
        public string? Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime? UpdatedAt { get; set; }
        public required List<PostReaction> Reactions { get; set; }
    }
    public class CreatePostDTO
    {
        public int Id { get; set; }
        public required string Content { get; set; }
    }
    public class DeletePostDTO
    {
        public int Id { get; set; }
    }
    public class UpdatePostDTO
    {
        public int Id { get; set; }
        public required string NewContent { get; set; }
    }
    public class CreateReactionDTO
    {
        public int PostId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ReactionType Type { get; set; }
    }
    public class ReactionAggregateDTO
    {
        public int PostId { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Hearts { get; set; }
    }
}