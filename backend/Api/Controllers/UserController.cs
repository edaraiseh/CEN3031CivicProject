using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Identity.Data;
using Api.Password;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // base route: /api/example
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        public UsersController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var userDTO = await _userService.GetUserByIdAsync(id);
            if (userDTO == null) return NotFound();
            return Ok(userDTO);
        }

        // POST api/example
        [HttpPut("register")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest userInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string? errMessage;
            var isValidPassword = PasswordHelper.IsValidPassword(userInfo.Password, out errMessage);
            if (!isValidPassword)
            {
                return BadRequest(new { message = errMessage });
            }

            var user = await _userService.RegisterUserAsync(userInfo);
            if (user == null) 
            {
                return Conflict( new { message = "Username is already taken" });
            }
            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Profile.Name,
                ProfilePic = user.Profile.Pic,
                Bio = user.Profile.Bio
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.AuthenticateUserAsync(userInfo);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }
            var token = _jwtService.GenerateJwt(user);
            return Ok(new UserLoginResponse {
                Token = token,
                UserId = user.Id
            });
        }
    }
}
