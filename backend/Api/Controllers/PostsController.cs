using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Api.Models;
using Api.Services;
using System.Numerics;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.ServiceUtils;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // base route: /api/example
    public class PostsController : ControllerBase
    {
        private readonly PostsService _postsService;
        public PostsController(PostsService postsService)
        {
            _postsService = postsService;
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "createdat",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int? userId = null,
            [FromQuery] string? search = null
        )
        {
            var (returnCode, posts) = await _postsService.GetPostsAsync(
                page, pageSize,
                sortBy, sortOrder,
                userId,
                search,
                false
            );
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(posts);
        }
        [HttpGet("official")]
        public async Task<IActionResult> GetOfficialPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "createdat",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int? userId = null,
            [FromQuery] string? search = null
        )
        {
            var (returnCode, posts) = await _postsService.GetPostsAsync(
                page, pageSize,
                sortBy, sortOrder,
                userId,
                search,
                true
            );
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(posts);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO post)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, createdPost) = await _postsService.CreatePostAsync(userId, post.Content);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(createdPost);
        }
        [HttpPut("official")]
        [Authorize]
        public async Task<IActionResult> CreateOfficialPost([FromBody] CreatePostDTO post)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, createdPost) = await _postsService.CreateOfficialPostAsync(userId, post.Content);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(createdPost);
        }

        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, deletedPost) = await _postsService.DeletePostAsync(userId, postId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(deletedPost);
        }

        [HttpPost("{postId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] UpdatePostDTO post)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, updatedPost) = await _postsService.UpdatePostAsync(userId, postId, post.NewContent);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(updatedPost);
        }

        [HttpPut("{parentId}/replies")]
        [Authorize]
        public async Task<IActionResult> CreateReply(int parentId, [FromBody] CreatePostDTO post)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, createdPost) = await _postsService.CreateReplyAsync(userId, parentId, post.Content);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(createdPost);
        }

        [HttpGet("{parentId}/replies")]
        public async Task<IActionResult> GetReplies(int parentId)
        {
            var (returnCode, posts) = await _postsService.GetRepliesAsync(parentId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(posts);
        }
        [HttpPut("{postId}/reactions")]
        [Authorize]
        public async Task<IActionResult> CreateReaction(int postId, [FromBody] CreateReactionDTO react)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);
            var (returnCode, reaction) = await _postsService.CreateReactionAsync(userId, postId, react.Type);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(reaction);
        }
        [HttpDelete("{postId}/reactions")]
        [Authorize]
        public async Task<IActionResult> DeleteReaction(int postId, [FromBody] CreateReactionDTO react)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);
            var (returnCode, reaction) = await _postsService.DeleteReactionAsync(userId, postId, react.Type);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(reaction);
        }
        [HttpGet("{postId}/reactions")]
        [Authorize]
        // As of right now, this doesn't use any redis caching. For the sake of developing all features on time,
        // it simply queries database every time. Oops.
        public async Task<IActionResult> GetReactionAggregates(int postId)
        {
            var (returnCode, reactions) = await _postsService.GetReactionAggregatesAsync(postId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(reactions);
        }
    }
}
