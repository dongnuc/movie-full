using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Events
{
    public sealed class BookingConfirmedNotificationHandler : INotificationHandler<BookingConfirmedNotification>
    {
        private readonly IBackgroundJobService _backgroundJobService;

        public BookingConfirmedNotificationHandler(IBackgroundJobService backgroundJobService)
        {
            _backgroundJobService = backgroundJobService;
        }

        public async Task Handle(BookingConfirmedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                _backgroundJobService.EnqueueEmailSending(notification.BookingId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire Queue Error] Failed to enqueue email job for booking {notification.BookingId}: {ex.Message}");
            }
        }
    }
}
