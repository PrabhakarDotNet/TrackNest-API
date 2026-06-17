using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackNest.Application.DTOs;
using TrackNest.Domain.Entities;

namespace TrackNest.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto?> LoginAsync(UserLoginDto loginDto, CancellationToken cancellationToken = default);
        Task<int> SignupAsync(UserSignupDto signupDto, CancellationToken cancellationToken = default);
        Task<User?> ValidateUser(string username, string password, CancellationToken cancellationToken = default);
        Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
