using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    /// <summary>
    /// Token provider implementation - x? lý JWT access token generation, validation, decode
    /// Ch?u trách nhi?m: T?o JWT token, validate signature, decode token
    /// </summary>
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public TokenProvider(UserManager<AppUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        /// <summary>
        /// Generate JWT access token v?i claims: UserId, Username, Email, Roles, Jti
        /// Ký b?ng HS256 v?i secret key t? configuration
        /// </summary>
        public async Task<string> GenerateAccessTokenAsync(AppUser user,string jti)
        {
            try
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                };

                // Get user roles from UserManager
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured")));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["JWT:ValidIssuer"],
                    audience: _config["JWT:ValidAudience"],
                    claims: authClaims,
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["JWT:ExpirationMinutes"] ?? "60")),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating access token", ex);
            }
        }

        /// <summary>
        /// Validate JWT token signature, issuer, audience, lifetime
        /// Return ClaimsPrincipal n?u valid, null n?u invalid
        /// </summary>
        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured"));

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["JWT:ValidAudience"],
                    ValidateLifetime = false,  
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Verify algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Decode token ?? l?y claims mà không validate lifetime
        /// Dùng cho refresh token flow
        /// </summary>
        public ClaimsPrincipal? DecodeToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured"));

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["JWT:ValidAudience"],
                    ValidateLifetime = false, // Không check expiration khi decode
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Verify algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

    }
}
