using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    public interface IRefreshTokenService
    {
        double GetAccessTokenExpirationMinutes();
        double GetRefreshTokenExpirationDays();

        Task<bool> RevokeRefreshTokenAsync(RefreshToken token);
        Task<bool> RevokeAllRefreshTokensAsync(Guid userId);
        Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti);

        Task<Result<RefreshAccessTokenResponse>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<LoginResponse>> GenerateTokenPairAndSaveAsync(AppUser user);
    }
}
