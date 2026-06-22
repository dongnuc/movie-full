using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Bookings.Queries.GetBookingsByShowing
{
    internal sealed class GetBookingsByShowingQueryHandler : IQueryHandler<GetBookingsByShowingQuery, List<ShowingBookingResponse>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IShowingRepository _showingRepository;

        public GetBookingsByShowingQueryHandler(IBookingRepository bookingRepository, IShowingRepository showingRepository)
        {
            _bookingRepository = bookingRepository;
            _showingRepository = showingRepository;
        }

        public async Task<Result<List<ShowingBookingResponse>>> Handle(GetBookingsByShowingQuery request, CancellationToken cancellationToken)
        {
            var showing = await _showingRepository.GetByIdAsync(request.ShowingId, cancellationToken);
            if (showing == null)
            {
                return Result.Failure<List<ShowingBookingResponse>>(
                    new Error("Showings.NotFound", $"Showing with ID '{request.ShowingId}' was not found."));
            }

            if (request.UserTheaterId.HasValue && showing.TheaterId != request.UserTheaterId.Value)
            {
                return Result.Failure<List<ShowingBookingResponse>>(
                    new Error("Showings.Forbidden", "You do not have permission to access showing bookings from this theater."));
            }

            var bookings = await _bookingRepository.GetBookingsByShowingIdAsync(request.ShowingId, cancellationToken);

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

                return new ShowingBookingResponse(
                    b.Id,
                    b.AppUser?.UserName ?? "Unknown",
                    b.AppUser?.Email ?? "Unknown",
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
