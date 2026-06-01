using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Register
{
    public static class Register
    {
        /// <summary>
        /// Register command - t?o tài kho?n m?i
        /// </summary>
        public sealed record Command(
            string  Username,
            string  Email,
            string  Password,
            string  ConfirmPassword
        ) : ICommand<string>;
    }
}
