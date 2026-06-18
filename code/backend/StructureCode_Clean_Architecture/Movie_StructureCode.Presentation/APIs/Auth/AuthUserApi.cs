using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.JsonWebTokens;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ConfirmEmail;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Logout;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Register;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ResendConfirmEmail;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Auth
{
    public class AuthUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/auth";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("User - Auth")
                .MapGroup(BaseUrl)
                .HasApiVersion(1);

            // ── REGISTER ─────────────────────────────────────────────────
            group.MapPost("/register", RegisterAsync)
                .WithName("Register")
                .WithSummary("Đăng ký tài khoản mới (username, email, password, re-password)")
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .AllowAnonymous();

            // ── CONFIRM EMAIL ────────────────────────────────────────────
            group.MapPost("/confirm-email", ConfirmEmailAsync)
                .WithName("ConfirmEmail")
                .WithSummary("Xác minh email tài khoản")
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .AllowAnonymous();

            // ── RESEND CONFIRM EMAIL ─────────────────────────────────────
            group.MapPost("/resend-confirm-email", ResendConfirmEmailAsync)
                .WithName("ResendConfirmEmail")
                .WithSummary("Gửi lại email xác thực")
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .AllowAnonymous();

            // ── LOGIN ────────────────────────────────────────────────────
            group.MapPost("/login", LoginAsync)
                .WithName("Login")
                .WithSummary("Đăng nhập (username hoặc email + password)")
                .Produces<LoginResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .AllowAnonymous();

            // ── REFRESH TOKEN ───────────────────────────────────────────
            group.MapPost("/refresh", RefreshAccessTokenAsync)
                .WithName("RefreshAccessToken")
                .WithSummary("Cấp access token mới từ refresh token")
                .Produces<RefreshAccessTokenResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .AllowAnonymous();

            // ── LOGOUT ───────────────────────────────────────────────────
            group.MapPost("/logout", LogoutAsync)
                .WithName("Logout")
                .WithSummary("Đăng xuất (xóa refresh token và session khỏi Redis)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .RequireAuthorization();
        }

        // ── HANDLERS ─────────────────────────────────────────────────────

        private static async Task<IResult> RegisterAsync(
            ISender sender,
            [FromBody] RegisterRequest request)
        {
            var command = new Register.Command(
                request.Username,
                request.Email,
                request.Password,
                request.ConfirmPassword);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : HandlerFailure(result);
        }

        private static async Task<IResult> ConfirmEmailAsync(
            ISender sender,
            [FromQuery] Guid userId,
            [FromQuery] string token)
        {
            var command = new ConfirmEmail.Command(userId, token);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : HandlerFailure(result);
        }

        private static async Task<IResult> ResendConfirmEmailAsync(
            ISender sender,
            [FromBody] ResendConfirmEmailRequest request)
        {
            var command = new ResendConfirmEmail.Command(request.Email);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : HandlerFailure(result);
        }

        private static async Task<IResult> LoginAsync(
            ISender sender,
            HttpContext httpContext,
            [FromBody] LoginRequest request)
        {
            var query = new Login.Query(request.UsernameOrEmail, request.Password);
            var result = await sender.Send(query);

            if (result.IsFailure)
                return HandlerFailure(result);
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = httpContext.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(1) // Adjust as needed, should match refresh token expiration
            };

            httpContext.Response.Cookies.Delete("refreshToken", options);
            httpContext.Response.Cookies.Append("refreshToken", result.Value!.RefreshToken, options);


            return Results.Ok(new
            {
                accessToken = result.Value.AccessToken,
                accessTokenExpiresAt = result.Value.AccessTokenExpiresAt
            });
        }

        private static async Task<IResult> RefreshAccessTokenAsync(
            ISender sender,
            HttpContext httpContext)
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"] ?? string.Empty;

            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();

            var command = new RefreshAccessToken.Command(refreshToken);

            var result = await sender.Send(command);

            if (result.IsFailure)
                return HandlerFailure(result);

            var deleteOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                Path = "/",
                SameSite = httpContext.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax
            };
            httpContext.Response.Cookies.Delete("refreshToken", deleteOptions);
            httpContext.Response.Cookies.Append("refreshToken", result.Value!.refreshTokenNew, new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                Path = "/",
                SameSite = httpContext.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = result.Value.refreshTokenExpires
            });

            return Results.Ok(new
            {
                accessToken = result.Value.AccessToken,
                accessTokenExpiresAt = result.Value.AccessTokenExpiresAt
            });
        }

        private static async Task<IResult> LogoutAsync(
            ISender sender,
            HttpContext httpContext)
        {
            var user = httpContext.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                return Results.Unauthorized();

            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
                return Results.Unauthorized();


            var command = new Logout.Command(
               Guid.Parse(userId),
               jti
           );
            var result = await sender.Send(command);

            if (!result.IsSuccess)
                return HandlerFailure(result);

            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = httpContext.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/"
            });

            return Results.NoContent();
        }
    }

    // ── Request models ────────────────────────────────────────────────────────

    /// <summary>
    /// Register request - tạo tài khoản mới
    /// </summary>
    public sealed record RegisterRequest(
        string Username,
        string Email,
        string Password,
        string ConfirmPassword);

    /// <summary>
    /// Resend Confirm Email request - gửi lại email xác thực
    /// </summary>
    public sealed record ResendConfirmEmailRequest(
        string Email);

    /// <summary>
    /// Login request - đăng nhập bằng username hoặc email
    /// </summary>
    public sealed record LoginRequest(
        string UsernameOrEmail,
        string Password);
}
