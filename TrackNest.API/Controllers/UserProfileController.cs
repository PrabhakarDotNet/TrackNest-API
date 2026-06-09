using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;

namespace TrackNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var user = await _userProfileService.GetProfileAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            await _userProfileService.UpdateProfileAsync(dto);
            return Ok("Profile updated successfully");
        }
    }
}