using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login
{
    public static class Login
    {
        /// <summary>
        /// Login query - cho phép đăng nhập bằng username hoặc email
        /// </summary>
        public sealed record Query(
            string  UsernameOrEmail,    // Có thể là username hoặc email
            string  Password
        ) : IQuery<LoginResponse>;
    }

    /// <summary>
    /// Login response - trả về user info và tokens
    /// </summary>
    public sealed record LoginResponse(
        string   AccessToken,
        DateTime AccessTokenExpiresAt,
        string   RefreshToken
    );
}
