using Microsoft.AspNetCore.Mvc;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;

namespace TrackNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized("Invalid email or password.");

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                AccessToken = result.AccessToken,
                User = result.User
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token not found.");

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (result == null)
                return Unauthorized("Invalid refresh token.");

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                AccessToken = result.AccessToken
            });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] UserSignupDto signupDto)
        {
            var userId = await _authService.SignupAsync(signupDto);

            return Ok(new
            {
                Id = userId
            });
        }
    }
}