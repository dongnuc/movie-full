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

            try
            {
                var token = ExtractTokenFromHeader(context);
                if (string.IsNullOrEmpty(token))
                {
                    await HandleUnauthorized(context, "token_missing");
                    return;
                }

                var principal = _tokenProvider.DecodeToken(token, out var securityToken);
                if (principal == null)
                {
                    await HandleUnauthorized(context, "token_invalid");
                    return;
                }

                if (securityToken == null || securityToken.ValidTo < DateTime.UtcNow)
                {
                    await HandleUnauthorized(context, "token_expired");
                    return;
                }

                var jti = (securityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken)?.Id ?? (securityToken as JwtSecurityToken)?.Id;
                if (string.IsNullOrEmpty(jti))
                {
                    await HandleUnauthorized(context, "token_invalid");
                    return;
                }

                var isTokenValid = await _tokenCacheService.IsAccessTokenValidAsync(jti);
                if (!isTokenValid)
                {
                    await HandleUnauthorized(context, "token_revoked");
                    return;
                }

                context.User = principal;

                var userId = principal.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Items["UserId"] = userId;
                }

                context.Items["JTI"] = jti;
                context.Items["ClaimsPrincipal"] = principal;

                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation");
                await HandleUnauthorized(context, "token_invalid");
            }
        }

        private static bool HasAllowAnonymousAttribute(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
                return false;

            return endpoint.Metadata
                .OfType<AllowAnonymousAttribute>()
                .Any();
        }

        private static string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
                return null;

            const string bearerScheme = "Bearer ";
            if (!authHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring(bearerScheme.Length).Trim();
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
