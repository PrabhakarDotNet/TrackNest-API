using Microsoft.EntityFrameworkCore;
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

        public AuthService(
            TrackNestDbContext dbContext,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
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
                .FirstOrDefaultAsync(x => x.Username == signupDto.Username, ct);

            if (existingUser != null)
                throw new Exception("User already exists");

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
                    Email = user.Email
                }
            };
        }
    }
}