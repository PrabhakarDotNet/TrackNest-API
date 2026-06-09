using TrackNest.Application.DTOs;
using TrackNest.Domain.Entities;

namespace TrackNest.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(int userId);
        Task UpdateProfileAsync(UserProfileDto profileDto);
    }
}
