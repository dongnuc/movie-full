using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs;
using Movie_StructureCode.Application.Abstractions.Services.Email;

namespace Movie_StructureCode.Application.Services.BackgroundJobs
{
    public class ConfirmationEmailJob : IConfirmationEmailJob
    {
        private readonly IEmailServices _emailServices;

        public ConfirmationEmailJob(IEmailServices emailServices)
        {
            _emailServices = emailServices;
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink, CancellationToken cancellationToken)
        {
            try
            {
                await _emailServices.SendMailConfirmAsync(email, confirmationLink);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Confirmation Email Job Exception] Failed to send registration email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}
