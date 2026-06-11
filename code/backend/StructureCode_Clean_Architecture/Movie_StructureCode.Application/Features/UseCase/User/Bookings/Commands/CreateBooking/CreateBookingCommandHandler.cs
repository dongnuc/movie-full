using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Application.Features.UseCase.User.Bookings.Events;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Commands.CreateBooking
{
    internal sealed class CreateBookingCommandHandler : ICommandHander<CreateBookingCommand, Guid>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingDetailRepository _bookingDetailRepository;
        private readonly IShowingSeatRepository _showingSeatRepository;
        private readonly IShowingRepository _showingRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IBookingDetailRepository bookingDetailRepository,
            IShowingSeatRepository showingSeatRepository,
            IShowingRepository showingRepository,
            ISeatRepository seatRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _bookingRepository = bookingRepository;
            _bookingDetailRepository = bookingDetailRepository;
            _showingSeatRepository = showingSeatRepository;
            _showingRepository = showingRepository;
            _seatRepository = seatRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Result<Guid>> Handle(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            if (request.SeatIds == null || !request.SeatIds.Any())
            {
                return Result.Failure<Guid>(new Error("Booking.NoSeatsSelected", "Vui lòng chọn ít nhất một ghế."));
            }

            // Begin transaction using UnitOfWork
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Retrieve existing showing seat records (which represents booked or active locks)
                var existingShowingSeats = await _showingSeatRepository.GetShowingSeatsForBooking(
                    request.ShowingId,
                    request.SeatIds,
                    cancellationToken);

                // Fetch physical seat configurations (to calculate price for available seats)
                var physicalSeats = await _seatRepository.GetSeatsWithTypesAsync(
                    request.SeatIds,
                    cancellationToken);

                if (physicalSeats.Count != request.SeatIds.Count)
                {
                    return Result.Failure<Guid>(new Error("Booking.SeatsNotFound", "Một hoặc nhiều ghế được chọn không tồn tại trong hệ thống."));
                }

                // Fetch showing details
                var showing = await _showingRepository.GetByIdAsync(request.ShowingId, cancellationToken);
                if (showing == null)
                {
                    return Result.Failure<Guid>(new Error("Booking.ShowingNotFound", "Suất chiếu không tồn tại."));
                }

                var existingShowingSeatsMap = existingShowingSeats.ToDictionary(ss => ss.SeatId);
                var physicalSeatsMap = physicalSeats.ToDictionary(s => s.Id);

                var bookingId = Guid.NewGuid();
                var showingSeatsToBook = new List<ShowingSeat>();
                decimal totalAmount = 0;

                // Validate and prepare seats to book
                foreach (var seatId in request.SeatIds)
                {
                    var seat = physicalSeatsMap[seatId];
                    decimal seatPrice = showing.Price + (seat.SeatType?.Surcharge ?? 0);

                    if (existingShowingSeatsMap.TryGetValue(seatId, out var showingSeat))
                    {
                        return Result.Failure<Guid>(new Error("Booking.SeatAlreadyBooked", $"Ghế {seat.RowName}{seat.SeatNumber} đã được đặt bởi người khác."));
                    }
                    else
                    {
                        // Create a new showing seat record for this available seat
                        var newShowingSeat = ShowingSeat.Create(
                            Guid.NewGuid(),
                            ShowingSeatStatus.Booked,
                            seatPrice,
                            lockedUntil: null,
                            showingId: request.ShowingId,
                            seatId: seatId,
                            bookingId: bookingId
                        );

                        await _showingSeatRepository.AddAsync(newShowingSeat, cancellationToken);
                        showingSeatsToBook.Add(newShowingSeat);
                        totalAmount += seatPrice;
                    }
                }

                // Create and save Booking
                var booking = Booking.Create(bookingId, totalAmount, StatusBooking.Confirmed, request.UserId, request.ShowingId);
                await _bookingRepository.AddAsync(booking, cancellationToken);

                // Create BookingDetails linked to booked showing seats
                foreach (var showingSeat in showingSeatsToBook)
                {
                    var bookingDetail = BookingDetail.Create(
                        Guid.NewGuid(),
                        showingSeat.Price,
                        BookingDetailStatus.Active,
                        bookingId,
                        showingSeat.Id);

                    await _bookingDetailRepository.AddAsync(bookingDetail, cancellationToken);
                }

                // Save all changes to database
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                // Publish Domain Event (outside transaction)
                await _mediator.Publish(new BookingConfirmedNotification(bookingId), cancellationToken);

                return Result.Success(bookingId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
        }
    }
}
