using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.Auth;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken
{
    public sealed class RefreshAccessTokenHandler : ICommandHander<RefreshAccessToken.Command, RefreshAccessTokenResponse>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshAccessTokenHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<RefreshAccessTokenResponse>> Handle(
            RefreshAccessToken.Command command,
            CancellationToken cancellationToken)
        {
            return await _refreshTokenService.RefreshAccessTokenAsync(command.RefreshToken, cancellationToken);
        }
    }
}
