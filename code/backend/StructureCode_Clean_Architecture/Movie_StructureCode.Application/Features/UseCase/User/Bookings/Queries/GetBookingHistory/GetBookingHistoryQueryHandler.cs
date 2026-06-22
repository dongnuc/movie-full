using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingHistory
{
    public sealed class GetBookingHistoryQueryHandler : IQueryHandler<GetBookingHistoryQuery, List<BookingHistoryResponse>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingHistoryQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<List<BookingHistoryResponse>>> Handle(GetBookingHistoryQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetBookingHistoryByUserIdAsync(request.UserId, cancellationToken);
            
            var response = bookings.Select(b =>
            {
                var seats = new List<string>();
                if (b.BookingDetails != null)
                {
                    foreach (var detail in b.BookingDetails)
                    {
                        var seat = detail.ShowingSeat?.Seat;
                        if (seat != null)
                        {
                            seats.Add($"{seat.RowName}{seat.SeatNumber}");
                        }
                    }
                }

                return new BookingHistoryResponse(
                    b.Id,
                    b.Showing?.Movie?.Title ?? "Unknown Movie",
                    b.Showing?.Movie?.ImagePoster ?? "",
                    b.Showing?.TimeStart ?? DateTime.MinValue,
                    b.Showing?.Room?.Name ?? "N/A",
                    b.Showing?.Room?.Theater?.Name ?? "N/A",
                    b.TotalAmount,
                    b.StatusBooking.ToString(),
                    seats,
                    b.DateCreate
                );
            }).ToList();

            return Result.Success(response);
        }
    }
}
