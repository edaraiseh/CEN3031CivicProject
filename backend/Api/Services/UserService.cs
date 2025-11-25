using Api.Data;
using Api.Models;
using Api.Password;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class UserService
    {
        private readonly TVDbContext _context;

        public UserService(TVDbContext context)
        {
            _context = context;
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            return await _context
                .Users
                .Include(u => u.Profile)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Profile.Name,
                    Bio = u.Profile.Bio,
                    ProfilePic = u.Profile.Pic
                })
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context
                .Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> RegisterUserAsync(CreateUserRequest userInfo)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userInfo.Username))
                return null;

            var (hash, salt) = PasswordHelper.CreatePasswordHash(userInfo.Password);

            // eventually, add some type of email verification

            var user = new User
            {
                Username = userInfo.Username,
                Email = userInfo.Email,
                Auth = new UserAuth
                {
                    PasswordHash = hash,
                    PasswordSalt = salt
                },
                Profile = new UserProfile
                {
                    Name = userInfo.Name,
                    Pic = userInfo.ProfilePic,
                    Bio = userInfo.Bio 
                }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> AuthenticateUserAsync(UserLoginRequest userInfo)
        {
            var user = await _context.Users
                .Include(u => u.Auth)
                .SingleOrDefaultAsync(u => u.Username == userInfo.Username);
            if (user == null)
            {
                return null;
            }

            if (DateTime.Now < user.Auth.LockUntil)
            {
                return null;
            }

            bool verified = PasswordHelper.VerifyPasswordHash(userInfo.Password, user.Auth.PasswordHash, user.Auth.PasswordSalt);
            if (!verified)
            {
                if (++user.Auth.FailedAttempts >= 10)
                {
                    user.Auth.LockUntil = DateTime.UtcNow.AddMinutes(5 * (user.Auth.FailedAttempts - 9));
                }
                await _context.SaveChangesAsync();
                return null;
            }
            user.Auth.LastLogin = DateTime.UtcNow;
            user.Auth.FailedAttempts = 0;
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
