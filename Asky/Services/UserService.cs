using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Asky.Services
{
    public interface IUserService
    {
        Task<IEnumerable<AdminUserDto>> GetUsers();

        Task<ViewProfileDto> GetProfileView(string username);

        Task<ProfileDto> GetUserProfile(string userId);

        Task<IEnumerable<TopicResultDto>> GetMyBookmarks(string userId);

        Task<IEnumerable<TopicResultDto>> GetMyUpVotes(string userId);

        Task<IEnumerable<TopicResultDto>> GetMyDownVotes(string userId);

        Task<IEnumerable<TopicResultDto>> GetHistory(string userId);

        Task<ApplicationUser> GetUserById(string userId);

        /// <returns>Authentication Token</returns>
        Task<string> Authenticate(string email, string password);

        /// <summary>
        /// Creates a new user, and then log him in.
        /// </summary>
        /// <returns>Authentication Token</returns>
        Task<string> Create(RegisterDto registerDto);

        Task ForgetPassword(string email);

        /// <summary>
        /// Checks for the token validation, reset the password, and log the user in.
        /// </summary>
        /// <returns>Authentication Token</returns>
        Task<string> ResetPassword(ResetPasswordDto resetPasswordDto);

        Task<string> UpdateProfile(string userId, UpdateProfileDto updateProfileDto);

        Task<string> UpdateProfileImage(string userId, ProfileImageDto profileImageDto);

        Task<string> UpdatePassword(string userId, string oldPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _sender;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context, IConfiguration configuration, IEmailSender sender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _sender = sender;
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsers()
        {
            var users = await _context.Users
                .Take(100)
                .Include(u => u.Topics)
                .Include(u => u.Votes)
                .ToListAsync();

            return users.Select(AdminUserDto.Create);
        }

        public async Task<ViewProfileDto> GetProfileView(string username)
        {
            var user = await GetUserByUsername(username);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            user.Topics = await _context.Topics.Where(t => t.UserId.Equals(user.Id) && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .Include(t => t.Views)
                .ToListAsync();

            return ViewProfileDto.Create(user);
        }

        public async Task<ProfileDto> GetUserProfile(string userId)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            return ProfileDto.Create(user);
        }

        public async Task<IEnumerable<TopicResultDto>> GetMyBookmarks(string userId)
        {
            var bookmarks = await _context.Bookmarks.Where(b => b.UserId.Equals(userId))
                .Include(b => b.Topic)
                .Select(b => b.Topic)
                .Where(t => !t.IsDeleted)
                .Distinct()
                .OrderByDescending(t => t.CreatedAt)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.Comments)
                .Include(b => b.Views)
                .ToListAsync();

            return bookmarks.Select(TopicResultDto.Create);
        }

        public async Task<IEnumerable<TopicResultDto>> GetMyUpVotes(string userId)
        {
            var votes = await _context.Votes.Where(v => v.UserId.Equals(userId) && v.IsUp)
                .Include(b => b.Topic)
                .Select(b => b.Topic)
                .Where(t => !t.IsDeleted)
                .Distinct()
                .OrderByDescending(t => t.CreatedAt)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.Comments)
                .Include(b => b.Views)
                .ToListAsync();

            return votes.Select(TopicResultDto.Create);
        }

        public async Task<IEnumerable<TopicResultDto>> GetMyDownVotes(string userId)
        {
            var votes = await _context.Votes.Where(v => v.UserId.Equals(userId) && !v.IsUp)
                .Include(v => v.Topic)
                .Select(v => v.Topic)
                .Where(t => !t.IsDeleted)
                .Distinct()
                .OrderByDescending(t => t.CreatedAt)
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .Include(t => t.Views)
                .ToListAsync();

            return votes.Select(TopicResultDto.Create);
        }

        public async Task<IEnumerable<TopicResultDto>> GetHistory(string userId)
        {
            var views = await _context.Views.Where(v => v.UserId.Equals(userId))
                .Include(v => v.Topic)
                .Select(v => v.Topic)
                .Where(t => !t.IsDeleted)
                .Distinct()
                .OrderByDescending(t => t.CreatedAt)
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .Include(t => t.Views)
                .ToListAsync();

            return views.Select(TopicResultDto.Create);
        }

        public async Task<string> Authenticate(string email, string password)
        {
            var user = await SignInByEmail(email, password);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            return await GenerateJwtToken(user);
        }

        public async Task<string> Create(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return await Authenticate(user.Email, registerDto.Password);
            }

            throw new ArgumentException(result.Errors.FirstOrDefault()?.Description);
        }

        public async Task ForgetPassword(string email)
        {
            var user = await GetUserByEmail(email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _sender.SendEmailAsync(user.Email, "Reset your password", token);
        }

        public async Task<string> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await GetUserByEmail(resetPasswordDto.Email);

            if (user == null)
            {
                throw new ArgumentException("Couldn't validate the token");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

            if (result.Succeeded)
            {
                return await Authenticate(user.Email, resetPasswordDto.Password);
            }

            throw new ArgumentException(result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<string> UpdateProfile(string userId, UpdateProfileDto updateProfileDto)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

//            if (!user.Email.Equals(updateProfileDto.Email))
//            {
//                var result = await _userManager.SetEmailAsync(user, updateProfileDto.Email);
//
//                if (!result.Succeeded)
//                    throw new ArgumentException("Failed to update");
//            }

            user.FirstName = updateProfileDto.FirstName;
            user.LastName = updateProfileDto.LastName;
            user.Email = updateProfileDto.Email;
            user.About = updateProfileDto.About;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new ArgumentException("Failed to update");

            return await GenerateJwtToken(user);
        }

        public async Task<string> UpdateProfileImage(string userId, ProfileImageDto profileImageDto)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var uri = ImageService.SaveImage(profileImageDto.Image);

            user.ImageUri = uri ?? throw new ArgumentException("Unable to upload the image");

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new ArgumentException("Failed to update");
            }

            return await GenerateJwtToken(user);
        }

        public async Task<string> UpdatePassword(string userId, string oldPassword, string newPassword)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
                throw new ArgumentException("Invalid Password");

            return await GenerateJwtToken(user);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = (await _userManager.GetRolesAsync(user)).Select(r => new Claim(nameof(ClaimTypes.Role), r)).ToList();

            claims.Add(new Claim(nameof(ClaimTypes.NameIdentifier).ToCamelCase(), user.Id));
            claims.Add(new Claim(nameof(ClaimTypes.Name).ToCamelCase(), user.UserName));
            claims.Add(new Claim(nameof(ClaimTypes.GivenName).ToCamelCase(), user.GetName()));
            claims.Add(new Claim(nameof(ClaimTypes.Email).ToCamelCase(), user.Email));
            claims.Add(new Claim(nameof(ClaimTypes.Uri).ToCamelCase(), user.GetImage()));

            var tokenHandler = new JwtSecurityTokenHandler();

            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));
            var credentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private async Task<ApplicationUser> SignInByEmail(string email, string password)
        {
            var user = await GetUserByEmail(email);

            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);

            return result.Succeeded ? user : null;
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        private async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        private async Task<ApplicationUser> GetUserByUsername(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }
    }
}
