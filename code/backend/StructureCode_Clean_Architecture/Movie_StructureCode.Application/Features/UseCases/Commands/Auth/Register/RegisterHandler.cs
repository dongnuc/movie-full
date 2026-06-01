using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Application.Abstractions.Services.Email;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Register
{
    public sealed class RegisterHandler : ICommandHander<Register.Command, string>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailServices _emailServices;

        public RegisterHandler(
            UserManager<AppUser> userManager,
            IEmailServices emailServices)
        {
            _userManager = userManager;
            _emailServices = emailServices;
        }

        public async Task<Result<string>> Handle(
            Register.Command command,
            CancellationToken cancellationToken)
        {
            // Validate password match
            if (command.Password != command.ConfirmPassword)
                return Result.Failure<string>(
                    new Error("Auth.PasswordMismatch", "Passwords do not match."));

            // Check if username already exists
            var userExists = await _userManager.FindByNameAsync(command.Username);
            if (userExists != null)
                return Result.Failure<string>(
                    new Error("Auth.UsernameTaken", $"Username '{command.Username}' is already taken."));

            // Check if email already exists
            var emailExists = await _userManager.FindByEmailAsync(command.Email);
            if (emailExists != null)
                return Result.Failure<string>(
                    new Error("Auth.EmailTaken", $"Email '{command.Email}' is already registered."));

            // Create new user
            var appUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = command.Username,
                Email = command.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(appUser, command.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<string>(
                    new Error("Auth.RegistrationFailed", $"Registration failed: {errors}"));
            }

            // Generate email confirmation token and send verification email
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var encodeToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

                // Build confirmation link with userId and token
                var confirmationLink = $"http://localhost:4200/confirm-email?userId={appUser.Id}&token={Uri.EscapeDataString(encodeToken)}";

                // Send confirmation email
                await _emailServices.SendMailConfirmAsync(appUser.Email, confirmationLink);
            }
            catch (Exception ex)
            {
                // Log error but don't fail registration
                // User can use resend email endpoint to request new confirmation email
                return Result.Success("Register success. Please check your email to verify your account. (Note: Could not send confirmation email, you can request it later)");
            }

            return Result.Success("Register success. Please check your email to verify your account.");
        }
    }
}
