using System.IdentityModel.Tokens.Jwt;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_StructureCode.API.Middleware
{
    public sealed class TokenValidationMiddleware : IMiddleware
    {
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly ITokenProvider _tokenProvider;
        private readonly ITokenCacheService _tokenCacheService;

        public TokenValidationMiddleware(
            ILogger<TokenValidationMiddleware> logger,
            ITokenProvider tokenProvider,
            ITokenCacheService tokenCacheService)
        {
            _logger = logger;
            _tokenProvider = tokenProvider;
            _tokenCacheService = tokenCacheService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await next(context);
                return;
            }

            if (HasAllowAnonymousAttribute(context) || ShouldSkipTokenValidation(context))
            {
                await next(context);
                return;
            }

            var requiresAuth = endpoint.Metadata.OfType<IAuthorizeData>().Any() &&
                               !endpoint.Metadata.OfType<IAllowAnonymous>().Any();

            string? token = null;
            ClaimsPrincipal? principal = null;
            SecurityToken? securityToken = null;
            string? jti = null;
            bool isTokenValid = false;

            try
            {
                token = ExtractToken(context);

                if (!string.IsNullOrEmpty(token))
                {
                    principal = _tokenProvider.DecodeToken(token, out securityToken);
                    if (principal != null && securityToken != null)
                    {
                        jti = (securityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken)?.Id ?? (securityToken as JwtSecurityToken)?.Id;
                        if (!string.IsNullOrEmpty(jti))
                        {
                            try
                            {
                                isTokenValid = await _tokenCacheService.IsAccessTokenValidAsync(jti);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Redis connection is down. Skipping cache-based validation. Error: {Message}", ex.Message);
                                isTokenValid = true; // Fallback to true if Redis is down
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown during token extraction or validation");
            }

            // Perform authentication challenge or proceed
            if (string.IsNullOrEmpty(token))
            {
                if (requiresAuth)
                {
                    await HandleUnauthorized(context, "token_missing");
                    return;
                }
            }
            else if (principal == null)
            {
                if (requiresAuth)
                {
                    await HandleUnauthorized(context, "token_invalid");
                    return;
                }
            }
            else if (securityToken == null || securityToken.ValidTo < DateTime.UtcNow)
            {
                if (requiresAuth)
                {
                    await HandleUnauthorized(context, "token_expired");
                    return;
                }
            }
            else if (string.IsNullOrEmpty(jti))
            {
                if (requiresAuth)
                {
                    await HandleUnauthorized(context, "token_invalid");
                    return;
                }
            }
            else if (!isTokenValid)
            {
                if (requiresAuth)
                {
                    await HandleUnauthorized(context, "token_revoked");
                    return;
                }
            }
            else
            {
                // Successful validation, assign identity context
                context.User = principal;

                var userId = principal.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Items["UserId"] = userId;
                }

                context.Items["JTI"] = jti;
                context.Items["ClaimsPrincipal"] = principal;
            }

            // Call next downstream middleware OUTSIDE of the token validation try-catch block!
            await next(context);
        }

        private static bool HasAllowAnonymousAttribute(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
                return false;

            return endpoint.Metadata
                .OfType<IAllowAnonymous>()
                .Any();
        }

        private static string? ExtractToken(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                const string bearerScheme = "Bearer ";
                if (authHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase))
                {
                    return authHeader.Substring(bearerScheme.Length).Trim();
                }
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase))
            {
                var accessToken = context.Request.Query["access_token"].ToString();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    return accessToken;
                }
            }

            return null;
        }

        private static async Task HandleUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = "Unauthorized",
                status = StatusCodes.Status401Unauthorized,
                detail = message,
                errors = (string[]?)null
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        private static bool ShouldSkipTokenValidation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            if (path.StartsWith("/swagger") || 
                path.StartsWith("/api/swagger") ||
                path.Contains("swagger.json"))
                return true;

            if (path == "/health" || path.StartsWith("/health/"))
                return true;

            if (path == "/favicon.ico")
                return true;

            return false;
        }
    }
}
