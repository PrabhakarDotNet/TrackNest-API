using Microsoft.EntityFrameworkCore;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Infrastructure.Persistence;

namespace TrackNest.Infrastructure.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly TrackNestDbContext _dbContext;

        public UserProfileService(TrackNestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task UpdateProfileAsync(UserProfileDto profileDto)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == profileDto.Id);

            if (user == null)
                throw new Exception("User not found");

            user.Username = profileDto.Username;
            user.Email = profileDto.Email;

            await _dbContext.SaveChangesAsync();
        }
    }
}