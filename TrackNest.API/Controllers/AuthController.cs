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
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] UserSignupDto signupDto)
        {
            var userId = await _authService.SignupAsync(signupDto);
            return Ok(new { Id = userId });
        }
    }
}