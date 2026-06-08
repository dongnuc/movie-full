using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public TokenProvider(UserManager<AppUser> userManager, IConfiguration config, AppDbContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
        }

        public async Task<string> GenerateAccessTokenAsync(AppUser user, string jti, string sessionId)
        {
            var tokenJti = !string.IsNullOrEmpty(jti) ? jti : Guid.NewGuid().ToString();
            try
            {
                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                    new Claim(JwtRegisteredClaimNames.Name, !string.IsNullOrEmpty(user.UserName) ? user.UserName : ""),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, tokenJti),
                    new Claim("token_type", "access")
                };

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    authClaims.Add(new Claim("role", role)); 
                }

                if (roles.Contains("ADMIN-THEATER", StringComparer.OrdinalIgnoreCase))
                {
                    var mapping = await _context.TheaterUsers
                        .Where(tu => tu.UserId == user.Id && tu.IsActive)
                        .OrderByDescending(tu => tu.IsPrimary)
                        .FirstOrDefaultAsync();

                    if (mapping != null)
                    {
                        authClaims.Add(new Claim("theaterId", mapping.TheaterId.ToString()));
                    }
                }

                var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is missing");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var issuer = _config["JWT:ValidIssuer"] ?? "MovieOwner";
                var audience = _config["JWT:ValidAudience"] ?? "MovieAL";
                var expireMinutes = double.Parse(_config["JWT:ExpirationMinutes"] ?? "60");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(authClaims),
                    Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                    SigningCredentials = creds,
                    Issuer = issuer,
                    Audience = audience
                };

                var tokenHandler = new JsonWebTokenHandler();
                return tokenHandler.CreateToken(tokenDescriptor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenProvider.GenerateAccessToken] Error: {ex.Message}");
                throw new InvalidOperationException("Error generating access token", ex);
            }
        }

        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            try
            {
                var secret = _config["JWT:Secret"];
                if (string.IsNullOrEmpty(secret)) return null;

                var key = Encoding.UTF8.GetBytes(secret);
                var issuer = _config["JWT:ValidIssuer"] ?? "MovieOwner";
                var audience = _config["JWT:ValidAudience"] ?? "MovieAL";

                var tokenHandler = new JsonWebTokenHandler();
                var validationResult = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                });

                if (!validationResult.IsValid || validationResult.SecurityToken is not JsonWebToken jwtToken)
                {
                    return null;
                }

                if (!jwtToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return new ClaimsPrincipal(validationResult.ClaimsIdentity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenProvider.ValidateAccessToken] Error: {ex.Message}");
                return null;
            }
        }

        public ClaimsPrincipal? DecodeToken(string token)
        {
            return DecodeToken(token, out _);
        }

        public ClaimsPrincipal? DecodeToken(string token, out SecurityToken? validatedToken)
        {
            validatedToken = null;
            try
            {
                var secret = _config["JWT:Secret"];
                if (string.IsNullOrEmpty(secret)) return null;

                var key = Encoding.UTF8.GetBytes(secret);
                var tokenHandler = new JsonWebTokenHandler();
                var validationResult = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                });

                if (!validationResult.IsValid || validationResult.SecurityToken is not JsonWebToken jwtToken)
                {
                    return null;
                }

                if (!jwtToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                validatedToken = jwtToken;
                return new ClaimsPrincipal(validationResult.ClaimsIdentity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenProvider.DecodeToken] Error: {ex.Message}");
                return null;
            }
        }

        public string GenerateRefreshToken(AppUser user, string refreshJti, string deviceId, DateTime expiresAt)
        {
            try
            {
                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, refreshJti),
                    new Claim("device_id", deviceId),
                    new Claim("token_type", "refresh")
                };

                var secret = _config["JWT:RefreshSecret"] ?? throw new InvalidOperationException("JWT:RefreshSecret is missing");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var issuer = _config["JWT:ValidIssuer"] ?? "MovieOwner";
                var audience = _config["JWT:ValidAudience"] ?? "MovieAL";

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(authClaims),
                    Expires = expiresAt,
                    SigningCredentials = creds,
                    Issuer = issuer,
                    Audience = audience
                };

                var tokenHandler = new JsonWebTokenHandler();
                return tokenHandler.CreateToken(tokenDescriptor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenProvider.GenerateRefreshToken] Error: {ex.Message}");
                throw new InvalidOperationException("Error generating refresh token", ex);
            }
        }

        public ClaimsPrincipal? DecodeRefreshToken(string token, out SecurityToken? validatedToken)
        {
            validatedToken = null;
            try
            {
                var secret = _config["JWT:RefreshSecret"];
                if (string.IsNullOrEmpty(secret)) return null;

                var key = Encoding.UTF8.GetBytes(secret);
                var tokenHandler = new JsonWebTokenHandler();
                var validationResult = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                });

                if (!validationResult.IsValid || validationResult.SecurityToken is not JsonWebToken jwtToken)
                {
                    return null;
                }

                if (!jwtToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                validatedToken = jwtToken;
                return new ClaimsPrincipal(validationResult.ClaimsIdentity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenProvider.DecodeRefreshToken] Error: {ex.Message}");
                return null;
            }
        }
    }
}