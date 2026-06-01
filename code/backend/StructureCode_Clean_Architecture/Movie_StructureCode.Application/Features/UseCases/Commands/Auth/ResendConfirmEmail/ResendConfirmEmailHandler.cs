using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Application.Abstractions.Services.Email;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.ResendConfirmEmail
{
    /// <summary>
    /// Resend Confirm Email Handler - Xử lý gửi lại email xác thực
    /// </summary>
    public sealed class ResendConfirmEmailHandler : ICommandHander<ResendConfirmEmail.Command, string>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailServices _emailServices;

        public ResendConfirmEmailHandler(
            UserManager<AppUser> userManager,
            IEmailServices emailServices)
        {
            _userManager = userManager;
            _emailServices = emailServices;
        }

        public async Task<Result<string>> Handle(
            ResendConfirmEmail.Command command,
            CancellationToken cancellationToken)
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure<string>(
                    new Error("Auth.InvalidEmail", "Email không được để trống."));

            if (!IsValidEmail(command.Email))
                return Result.Failure<string>(
                    new Error("Auth.InvalidEmailFormat", "Định dạng email không hợp lệ."));

            try
            {
                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(command.Email);
                if (user == null)
                    return Result.Failure<string>(
                        new Error("Auth.UserNotFound", "Người dùng không tồn tại."));

                // Kiểm tra xem email đã được xác minh rồi
                if (user.EmailConfirmed)
                    return Result.Failure<string>(
                        new Error("Auth.EmailAlreadyConfirmed", "Email của bạn đã được xác minh rồi."));

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodeToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

                // Build confirmation link with userId and token
                var confirmationLink = $"http://localhost:4200/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(encodeToken)}";

                // Send confirmation email
                await _emailServices.SendMailConfirmAsync(user.Email, confirmationLink);

                return Result.Success("Email xác thực đã được gửi. Vui lòng kiểm tra hộp thư của bạn.");
            }
            catch (Exception ex)
            {
                return Result.Failure<string>(
                    new Error("Auth.ResendEmailFailed", $"Không thể gửi email xác thực: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kiểm tra định dạng email có hợp lệ
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
