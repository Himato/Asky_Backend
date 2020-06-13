using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Asky.Models;
using Microsoft.AspNetCore.Http;

namespace Asky.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required, MinLength(4, ErrorMessage = "Username can't be less than 4 letters"), MaxLength(32, ErrorMessage = "Username can't be more than 32 letters")]
        [RegularExpression(@"^[\w]+$", ErrorMessage = "Username can only contain English letters, underscores, and numbers")]
        public string Username { get; set; }

        [Required, MinLength(4, ErrorMessage = "First Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "First Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain English letters, underscores, and spaces")]
        public string FirstName { get; set; }

        [Required, MinLength(4, ErrorMessage = "Last Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "Last Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain English letters, underscores, and spaces")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(4, ErrorMessage = "First Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "First Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain English letters, underscores, and spaces")]
        public string FirstName { get; set; }

        [Required, MinLength(4, ErrorMessage = "Last Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "Last Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain English letters and spaces")]
        public string LastName { get; set; }


        [MinLength(4, ErrorMessage = "About can't be less than 4 letters"), MaxLength(255, ErrorMessage = "About can't be more than 255 letters")]
        public string About { get; set; }
    }

    public class UpdatePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileImageDto
    {
        [Required]
        public IFormFile Image { get; set; }
    }

    public class ProfileDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string ImageUri { get; set; }

        public string About { get; set; }

        public static ProfileDto Create(ApplicationUser user)
        {
            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ImageUri = user.GetImage(),
                About = user.About
            };
        }
    }

    public class ViewUserDto
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }

        public static ViewUserDto Create(ApplicationUser user)
        {
            return new ViewUserDto
            {
                Username = user.UserName,
                Name = user.GetName(),
                ImageUri = user.GetImage()
            };
        }
    }
    
    public class ViewProfileDto
    {
        public string Name { get; set; }
        
        public string ImageUri { get; set; }

        public string About { get; set; }

        public IEnumerable<TopicResultDto> Topics { get; set; }

        public static ViewProfileDto Create(ApplicationUser user)
        {
            return new ViewProfileDto
            {
                Name = user.GetName(),
                ImageUri = user.GetImage(),
                About = user.About,
                Topics = user.Topics.Select(TopicResultDto.Create)
            };
        }
    }

    public class AdminUserDto
    {
        public string Name { get; set; }

        public string Username { get; set; }

        public string ImageUri { get; set; }

        public int NumberOfTopics { get; set; }

        public int NumberOfUpVotes { get; set; }

        public int NumberOfDownVotes { get; set; }

        public static AdminUserDto Create(ApplicationUser user)
        {
            return new AdminUserDto
            {
                Name = user.GetName(),
                Username = user.UserName,
                ImageUri = user.GetImage(),
                NumberOfTopics = user.Topics.Count,
                NumberOfUpVotes = user.Votes.Count(v => v.IsUp),
                NumberOfDownVotes = user.Votes.Count(v => !v.IsUp)
            };
        }
    }
}
