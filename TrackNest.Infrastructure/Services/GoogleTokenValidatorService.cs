using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackNest.Application.Interfaces;

namespace TrackNest.Infrastructure.Services
{
   public class GoogleTokenValidatorService : IGoogleTokenValidator
    {
        public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken, GoogleJsonWebSignature.ValidationSettings settings)
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
    }
}
