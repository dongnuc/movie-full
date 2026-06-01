using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ResendConfirmEmail
{
    /// <summary>
    /// Resend Confirm Email Command - G?i l?i email xác th?c
    /// </summary>
    public static class ResendConfirmEmail
    {
        public sealed record Command(
            string Email
        ) : ICommand<string>;
    }
}
