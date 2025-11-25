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
    public class PetitionsController : ControllerBase
    {
        private readonly PetitionsService _petitionsService;
        public PetitionsController(PetitionsService petitionsService)
        {
            _petitionsService = petitionsService;
        }


        [HttpGet]
        public async Task<IActionResult> GetPetitions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "createdat",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int? userId = null,
            [FromQuery] string? search = null
        )
        {
            var (returnCode, posts) = await _petitionsService.GetPetitionsAsync(
                page, pageSize,
                sortBy, sortOrder,
                userId,
                search
            );
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(posts);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CreatePetition([FromBody] CreatePetitionDTO petition)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, createdPetition) = await _petitionsService.CreatePetitionAsync(userId, petition.Title, petition.Content);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(createdPetition);
        }

        [HttpDelete("{petitionId}")]
        [Authorize]
        public async Task<IActionResult> DeletePetition(int petitionId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);

            var (returnCode, deletedPetition) = await _petitionsService.DeletePetitionAsync(userId, petitionId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(deletedPetition);
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

            var (returnCode, createdPost) = await _petitionsService.CreateReplyAsync(userId, parentId, post.Content);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(createdPost);
        }

        [HttpGet("{parentId}/replies")]
        public async Task<IActionResult> GetReplies(int parentId)
        {
            var (returnCode, replies) = await _petitionsService.GetRepliesAsync(parentId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(replies);
        }
        [HttpPut("{petitionId}/sign")]
        [Authorize]
        public async Task<IActionResult> CreateSignature(int petitionId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);
            var (returnCode, signature) = await _petitionsService.CreatePetitionSignatureAsync(userId, petitionId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(signature);
        }
        [HttpDelete("{petitionId}/sign")]
        [Authorize]
        public async Task<IActionResult> DeleteSignature(int petitionId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdStr);
            var (returnCode, reaction) = await _petitionsService.DeletePetitionSignatureAsync(userId, petitionId);
            var result = ServiceHelper.HandleReturnCode(returnCode);
            if (result is not OkResult) { return result; }
            return Ok(reaction);
        }
    }
}
