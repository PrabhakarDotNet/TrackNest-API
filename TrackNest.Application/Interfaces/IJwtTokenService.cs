using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackNest.Application.DTOs;
using TrackNest.Domain.Entities;

namespace TrackNest.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
