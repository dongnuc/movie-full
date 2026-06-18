using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingDetail
{
    internal sealed class GetBookingDetailQueryHandler : IQueryHandler<GetBookingDetailQuery, BookingDetailResponse>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingDetailQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<BookingDetailResponse>> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetBookingDetailAsync(request.BookingId, request.UserId, cancellationToken);
            if (booking == null)
            {
                return Result.Failure<BookingDetailResponse>(new Error("Booking.NotFound", "Không tìm thấy thông tin đặt vé."));
            }

            var details = booking.BookingDetails?
                .Select(bd => new BookingDetailItemResponse(
                    bd.Id,
                    bd.ShowingSeat?.Seat?.RowName ?? string.Empty,
                    bd.ShowingSeat?.Seat?.SeatNumber ?? 0,
                    bd.ShowingSeat?.Seat?.SeatType?.Name ?? "Normal",
                    bd.Price
                ))
                .ToList() ?? new System.Collections.Generic.List<BookingDetailItemResponse>();

            var response = new BookingDetailResponse(
                booking.Id,
                booking.Showing?.Movie?.Title ?? "Unknown Movie",
                booking.Showing?.Movie?.Image,
                booking.Showing?.TimeStart ?? System.DateTime.MinValue,
                booking.Showing?.Room?.Name ?? "N/A",
                booking.Showing?.Room?.Theater?.Name ?? "N/A",
                booking.Showing?.Room?.Theater?.Location ?? "N/A",
                booking.TotalAmount,
                booking.StatusBooking.ToString(),
                details,
                booking.DateCreate
            );

            return Result.Success(response);
        }
    }
}
