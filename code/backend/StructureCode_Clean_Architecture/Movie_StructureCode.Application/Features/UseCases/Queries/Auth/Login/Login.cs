using Movie_StructureCode.Contract.Abstractions.Message;
using System;

namespace Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login
{
    public static class Login
    {
        public sealed record Query(
            string UsernameOrEmail,
            string Password
        ) : IQuery<LoginResponse>;
    }

    public sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt
    );
}
