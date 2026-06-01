using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Logout
{
    public static class Logout
    {
        /// <summary>
        /// Logout command - ??ng xu?t b?ng access token
        /// Access token s? ???c extracted t? HttpContext b?i middleware
        /// </summary>
        public sealed record Command(
            Guid userId,
            string jti
        ) : ICommand<bool>;
    }
}
