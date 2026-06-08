using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            ITokenCacheService tokenCacheService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _tokenCacheService = tokenCacheService;
            _configuration = configuration;
        }

        public async Task<Result<LoginResponse>> LoginAsync(
            string usernameOrEmail,
            string password,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                ?? await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user == null)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Invalid username/email or password."));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.AccountLocked", "Your account has been locked. Please contact the administrator."));
            }

            if (!user.EmailConfirmed)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.EmailNotConfirmed", "Your email has not been verified yet. Please check your email and verify your account before logging in."));
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Invalid username/email or password."));
            }

            return await _tokenService.GenerateTokenPairAndSaveAsync(user);
        }
    }
}
