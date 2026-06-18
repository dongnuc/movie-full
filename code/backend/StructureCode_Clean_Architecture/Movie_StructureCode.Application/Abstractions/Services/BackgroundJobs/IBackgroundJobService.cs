using System;

namespace Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs
{
    public interface IBackgroundJobService
    {
        void EnqueueEmailSending(Guid bookingId);
        void EnqueueConfirmationEmail(string email, string confirmationLink);
    }
}
