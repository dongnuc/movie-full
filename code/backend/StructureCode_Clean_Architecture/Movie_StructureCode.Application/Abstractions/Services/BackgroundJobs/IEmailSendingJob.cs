using System;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs
{
    public interface IEmailSendingJob
    {
        Task SendEmailAsync(Guid bookingId, CancellationToken cancellationToken);
    }
}
