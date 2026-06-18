using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

public sealed class AuthenticationService : Movie_StructureCode.Application.Abstractions.Services.Auth.IAuthenticationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITokenCacheService _tokenCacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        ITokenCacheService tokenCacheService,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _tokenCacheService = tokenCacheService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> LoginAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.NormalizedUserName == usernameOrEmail.ToUpperInvariant()
                                   || u.NormalizedEmail == usernameOrEmail.ToUpperInvariant(), cancellationToken);

        _logger.LogDebug("[{User}] FindUser: {Ms}ms", usernameOrEmail, sw.ElapsedMilliseconds);

        if (user == null)
            return Result.Failure<LoginResponse>(
                new Error("Auth.InvalidCredentials", "Invalid username/email or password."));

        // Memory checks — không hit DB
        if (!user.EmailConfirmed)
            return Result.Failure<LoginResponse>(
                new Error("Auth.EmailNotConfirmed", "Your email has not been verified yet."));

        var isLocked = await _userManager.IsLockedOutAsync(user);
        if (isLocked)
            return Result.Failure<LoginResponse>(
                new Error("Auth.AccountLocked", "Your account has been locked."));

        // CPU-intensive — để cuối cùng
        sw.Restart();
        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        _logger.LogDebug("[{User}] CheckPassword: {Ms}ms", usernameOrEmail, sw.ElapsedMilliseconds);

        if (!passwordValid)
            return Result.Failure<LoginResponse>(
                new Error("Auth.InvalidCredentials", "Invalid username/email or password."));

        sw.Restart();
        var result = await _tokenService.GenerateTokenPairAndSaveAsync(user);
        _logger.LogDebug("[{User}] GenerateToken: {Ms}ms", usernameOrEmail, sw.ElapsedMilliseconds);

        return result;
    }
}