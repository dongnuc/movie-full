using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Application.Abstractions.Services.Cache;

namespace Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login
{
    public sealed class LoginHandler : IQueryHandler<Login.Query, LoginResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ITokenCacheService _tokenCacheService;


        public LoginHandler(
            UserManager<AppUser> userManager,
            ITokenCacheService tokenCacheService,
            ITokenService tokenService)
        {
            _tokenCacheService = tokenCacheService;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponse>> Handle(
            Login.Query request,
            CancellationToken cancellationToken)
        {
            // Try find user by username first, then by email
            var user = await _userManager.FindByNameAsync(request.UsernameOrEmail)
                ?? await _userManager.FindByEmailAsync(request.UsernameOrEmail);

            if (user == null)
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Invalid username/email or password."));

            // Check if account is locked out / deactivated
            if (await _userManager.IsLockedOutAsync(user))
                return Result.Failure<LoginResponse>(
                    new Error("Auth.AccountLocked", "Your account has been locked. Please contact the administrator."));

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                return Result.Failure<LoginResponse>(
                    new Error("Auth.EmailNotConfirmed", "Your email has not been verified yet. Please check your email and verify your account before logging in."));

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Invalid username/email or password."));

            // Generate tokens
            var jti = Guid.NewGuid(); // Unique identifier for the JWT, used for refresh token tracking
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, jti.ToString());   
            var refreshToken = _tokenService.GenerateRefreshToken();
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(2); // Adjust as needed
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(1); // Adjust as needed

            // Save refresh token
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(RefreshToken.Create(
                Guid.NewGuid(),
                refreshToken,
                jti.ToString(),
                refreshTokenExpiresAt,
                user.Id
            ));

            await _userManager.UpdateAsync(user);

            //Redis
            await _tokenCacheService.StoreAccessTokenAsync(jti.ToString(), user.Id.ToString(), accessTokenExpiresAt);
            await _tokenCacheService.AddUserSessionAsync(user.Id.ToString(), jti.ToString(), refreshTokenExpiresAt);

            

            var response = new LoginResponse(
                accessToken,
                accessTokenExpiresAt,
                refreshToken
            );

            return Result.Success(response);
        }
    }
}
