using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ConfirmEmail
{
    /// <summary>
    /// Confirm Email Handler - X? lý xác minh email tài kho?n
    /// </summary>
    public sealed class ConfirmEmailHandler : ICommandHander<ConfirmEmail.Command, string>
    {
        private readonly UserManager<AppUser> _userManager;

        public ConfirmEmailHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(
            ConfirmEmail.Command command,
            CancellationToken cancellationToken)
        {
            // Validate input
            if (command.UserId == Guid.Empty)
                return Result.Failure<string>(
                    new Error("Auth.InvalidUserId", "UserId không ???c ?? tr?ng."));

            if (string.IsNullOrWhiteSpace(command.Token))
                return Result.Failure<string>(
                    new Error("Auth.InvalidToken", "Token không ???c ?? tr?ng."));

            try
            {
                // Tìm user theo UserId
                var user = await _userManager.FindByIdAsync(command.UserId.ToString());
                if (user == null)
                    return Result.Failure<string>(
                        new Error("Auth.UserNotFound", "Ng??i dùng không t?n t?i."));

                // Ki?m tra xem email ?ã ???c xác minh ch?a
                if (user.EmailConfirmed)
                    return Result.Failure<string>(
                        new Error("Auth.EmailAlreadyConfirmed", "Email c?a b?n ?ã ???c xác minh r?i."));

                // Xác minh email b?ng token
                var decodeToken = Encoding.UTF8.GetString(Convert.FromBase64String(command.Token));
                var result = await _userManager.ConfirmEmailAsync(user, decodeToken);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure<string>(
                        new Error("Auth.ConfirmEmailFailed", $"Xác minh email th?t b?i: {errors}"));
                }

                return Result.Success("Email c?a b?n ?ã ???c xác minh thành công!");
            }
            catch (FormatException)
            {
                return Result.Failure<string>(
                    new Error("Auth.InvalidTokenFormat", "Token không có ??nh d?ng h?p l?."));
            }
            catch (Exception ex)
            {
                return Result.Failure<string>(
                    new Error("Auth.ConfirmEmailError", $"L?i khi xác minh email: {ex.Message}"));
            }
        }
    }
}
