using System.IdentityModel.Tokens.Jwt;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movie_StructureCode.API.Middleware
{
    /// <summary>
    /// Middleware ?? validate JWT token v?i Redis:
    /// 1. Extract token t? Authorization header
    /// 2. Validate token signature (basic validation)
    /// 3. Extract JTI (JWT ID) t? token claims
    /// 4. Ki?m tra token có t?n t?i và h?p l? trên Redis hay không
    /// 5. Ki?m tra phiên b?n token có kh?p v?i session hi?n t?i không
    /// 
    /// L?u ý: Middleware s? d?ng [AllowAnonymous] attribute ?? xác ??nh endpoints công khai
    /// Không s? d?ng hardcode danh sách endpoints
    /// </summary>
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
            // Ki?m tra xem endpoint có ???c mark v?i [AllowAnonymous] không
            // Ho?c là swagger/health endpoints không c?n token
            if (HasAllowAnonymousAttribute(context) || ShouldSkipTokenValidation(context))
            {
                await next(context);
                return;
            }

            try
            {
                // Extract token t? Authorization header
                var token = ExtractTokenFromHeader(context);

                if (string.IsNullOrEmpty(token))
                {
                    await next(context);
                    return;
                }

                // Validate token signature và format
                var principal = context.User;
                if (principal == null)
                {
                    await HandleUnauthorized(context, "Invalid token signature");
                    return;
                }

                // Extract JTI t? claims
                var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    await HandleUnauthorized(context, "Invalid token: JTI claim missing");
                    return;
                }

                // Extract userId t? claims
                var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await HandleUnauthorized(context, "Invalid token: UserId claim missing");
                    return;
                }

                // Ki?m tra token có t?n t?i trên Redis không
                var isTokenValid = await _tokenCacheService.IsAccessTokenValidAsync(jti);
                if (!isTokenValid)
                {
                    await HandleUnauthorized(context, "Token is invalid or expired");
                    return;
                }

                // Ki?m tra JTI có thu?c v? user session hi?n t?i không
                var userSessions = await _tokenCacheService.GetUserSessionsAsync(userId);
                if (!userSessions.Contains(jti))
                {
                    await HandleUnauthorized(context, "Token version mismatch: JTI not in active sessions");
                    return;
                }

                context.Items["UserId"] = userId;
                context.Items["JTI"] = jti;
                context.Items["ClaimsPrincipal"] = principal;

                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation");
                await HandleUnauthorized(context, "Token validation failed");
            }
        }

        /// <summary>
        /// Ki?m tra xem endpoint có ???c marked v?i [AllowAnonymous] attribute không
        /// Thay vì hardcode danh sách endpoints
        /// </summary>
        private static bool HasAllowAnonymousAttribute(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
                return false;

            // Ki?m tra endpoint metadata có ch?a AllowAnonymousAttribute không
            var hasAllowAnonymous = endpoint.Metadata
                .OfType<AllowAnonymousAttribute>()
                .Any();

            return hasAllowAnonymous;
        }

        /// <summary>
        /// Extract JWT token t? Authorization header
        /// Format: Authorization: Bearer {token}
        /// </summary>
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

        /// <summary>
        /// X? lý l?i unauthorized - tr? v? 401 response
        /// </summary>
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

        /// <summary>
        /// Ki?m tra xem request có ph?i là swagger/health endpoint không
        /// Nh?ng endpoint này không c?n token validation
        /// </summary>
        private static bool ShouldSkipTokenValidation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Swagger endpoints
            if (path.StartsWith("/swagger") || 
                path.StartsWith("/api/swagger") ||
                path.Contains("swagger.json"))
                return true;

            // Health check endpoint
            if (path == "/health" || path.StartsWith("/health/"))
                return true;

            // Favicon
            if (path == "/favicon.ico")
                return true;

            return false;
        }
    }
}
