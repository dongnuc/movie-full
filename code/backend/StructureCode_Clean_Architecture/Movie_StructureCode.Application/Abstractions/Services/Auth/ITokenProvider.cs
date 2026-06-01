using System.Security.Claims;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    /// <summary>
    /// Token provider interface - x? lý JWT access token generation, validation, decode
    /// Ch?u trách nhi?m: T?o token, validate signature, decode token
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Generate JWT access token cho user
        /// </summary>
        /// <param name="user">AppUser entity (contains userId, username, email, roles)</param>
        Task<string> GenerateAccessTokenAsync(AppUser user,string jti);

        /// <summary>
        /// Validate JWT token signature và lifetime
        /// </summary>
        ClaimsPrincipal? ValidateAccessToken(string token);

        /// <summary>
        /// Decode token và tr? v? claims (không check lifetime)
        /// Dùng cho refresh token flow
        /// </summary>
        ClaimsPrincipal? DecodeToken(string token);
    }
}
