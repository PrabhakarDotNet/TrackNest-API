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
        Task<AuthResultDto?> LoginAsync(UserLoginDto loginDto);
        Task<int> SignupAsync(UserSignupDto signupDto);
        Task<User?> ValidateUser(string username, string password);
        Task<AuthResultDto?> RefreshTokenAsync(string refreshToken);
    }
}
