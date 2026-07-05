using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;

namespace TrackNest.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly TrackNestDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly IGoogleTokenValidator _googleTokenValidator;

        public AuthService(
            TrackNestDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            IGoogleTokenValidator googleTokenValidator)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _googleTokenValidator = googleTokenValidator;
        }

        public async Task<AuthResultDto?> LoginAsync(UserLoginDto loginDto, CancellationToken ct = default)
        {
            var user = await ValidateUser(loginDto.Username, loginDto.Password, ct);

            if (user == null)
                return null;

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _dbContext.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                }
            };
        }

        public async Task<int> SignupAsync(UserSignupDto signupDto, CancellationToken ct = default)
        {
            var existingUser = await _dbContext.Users
                                .FirstOrDefaultAsync(
                                x => x.Username == signupDto.Username || x.Email == signupDto.Email, ct);

            if (existingUser != null)
            {
                if (existingUser.Username == signupDto.Username)
                    throw new Exception("Username already exists.");

                if (existingUser.Email == signupDto.Email)
                    throw new Exception("Email already exists.");
            }

            var user = new User
            {
                Username = signupDto.Username,
                Email = signupDto.Email,
                Password = signupDto.Password
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(ct);

            return user.Id;
        }

        public async Task<User?> ValidateUser(string username, string password, CancellationToken ct = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Username == username, ct);

            if (user == null)
                return null;

            if (user.Password != password)
                return null;

            return user;
        }

        public async Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, ct);

            if (user == null)
                return null;

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;

            var newAccessToken = _jwtTokenService.GenerateToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _dbContext.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName ?? user.Username
                }
            };
        }

        public async Task<AuthResultDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken ct = default)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                payload = await _googleTokenValidator.ValidateAsync(request.IdToken, settings);
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == payload.Email, ct);

            if (user == null)
            {
                var firstName = payload.Name?.Split(' ').FirstOrDefault() ?? payload.Email!.Split('@')[0];

                user = new User
                {
                    Email = payload.Email!,
                    Username = payload.Email!,
                    DisplayName = firstName,
                    GoogleId = payload.Subject,
                    AuthProvider = "Google",
                    Password = null
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(ct);
            }
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject;
                await _dbContext.SaveChangesAsync(ct);
            }

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _dbContext.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName ?? user.Username
                }
            };
        }
    }
}