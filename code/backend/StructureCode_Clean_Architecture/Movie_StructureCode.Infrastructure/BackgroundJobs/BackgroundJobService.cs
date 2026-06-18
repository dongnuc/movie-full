using System;
using Hangfire;
using Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs;

namespace Movie_StructureCode.Infrastructure.BackgroundJobs
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BackgroundJobService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void EnqueueEmailSending(Guid bookingId)
        {
            _backgroundJobClient.Enqueue<IEmailSendingJob>(job => job.SendEmailAsync(bookingId, default));
        }

        public void EnqueueConfirmationEmail(string email, string confirmationLink)
        {
            _backgroundJobClient.Enqueue<IConfirmationEmailJob>(job => job.SendConfirmationEmailAsync(email, confirmationLink, default));
        }
    }
}
