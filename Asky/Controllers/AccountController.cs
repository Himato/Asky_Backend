using System;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asky.Controllers
{
    public class AccountController : ApiHelper
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            return await Do(async () =>
            {
                var result = await _userService.Authenticate(loginDto.Email, loginDto.Password);

                if (result == null)
                {
                    throw new ArgumentException("Invalid Login Attempt");
                }

                return result;
            });
        }

        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            return await Create(nameof(Login), async () =>
            {
                var result = await _userService.Create(registerDto);

                if (result == null)
                {
                    throw new ArgumentException("Couldn't create your account");
                }

                return result;
            });
        }

        [HttpPost]
        [Route("ForgetPassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            return await Do(async () =>
            {
                await _userService.ForgetPassword(forgetPasswordDto.Email);
            });
        }

        [HttpPost]
        [Route("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            return await Do(async () =>
            {
                var result = await _userService.ResetPassword(resetPasswordDto);

                if (result == null)
                {
                    throw new ArgumentException("Invalid Attempt");
                }

                return result;
            });
        }

        [HttpGet]
        [Route("Profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            return await Do(async () => await _userService.GetUserProfile(User.Identity.GetUserId()));
        }

        [HttpPut]
        [Route("UpdateProfile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
        {
            return await Do(async () => await _userService.UpdateProfile(User.Identity.GetUserId(), updateProfileDto));
        }

        [HttpPut]
        [Route("UpdateImage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateImage([FromForm] ProfileImageDto profileImageDto)
        {
            return await Do(async () => await _userService.UpdateProfileImage(User.Identity.GetUserId(), profileImageDto));
        }

        [HttpPut]
        [Route("UpdatePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
        {
            return await Do(async () => await _userService.UpdatePassword(User.Identity.GetUserId(), updatePasswordDto.CurrentPassword, updatePasswordDto.NewPassword));
        }
    }
}
