using System;
using MediatR;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Events
{
    public sealed record BookingConfirmedNotification(Guid BookingId) : INotification;
}
