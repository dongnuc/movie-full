using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ConfirmEmail
{
    /// <summary>
    /// Confirm Email Command - Xác minh email ng??i dùng
    /// </summary>
    public static class ConfirmEmail
    {
        public sealed record Command(
            Guid UserId,
            string Token
        ) : ICommand<string>;
    }
}
