using Microsoft.IdentityModel.Tokens;
using Movie_StructureCode.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    public interface ITokenProvider
    {
        Task<string> GenerateAccessTokenAsync(AppUser user, string jti, string sessionId);
        ClaimsPrincipal? ValidateAccessToken(string token);
        ClaimsPrincipal? DecodeToken(string token);
        ClaimsPrincipal? DecodeToken(string token, out SecurityToken? validatedToken);

        // Refresh Token operations
        string GenerateRefreshToken(AppUser user, string refreshJti, string deviceId, DateTime expiresAt);
        ClaimsPrincipal? DecodeRefreshToken(string token, out SecurityToken? validatedToken);
    }
}
