using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Application.Features.UseCase.User.Bookings.Events;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;
using StackExchange.Redis;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Commands.CreateBooking
{
    public sealed class CreateBookingCommandHandler : ICommandHander<CreateBookingCommand, Guid>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingDetailRepository _bookingDetailRepository;
        private readonly IShowingSeatRepository _showingSeatRepository;
        private readonly IShowingRepository _showingRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly ISeatLockService _seatLockService;
        private readonly ISeatHubService _seatHubService;
        private readonly IConnectionMultiplexer _redis;
        private readonly IBookingSessionRepository _bookingSessionRepository;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IBookingDetailRepository bookingDetailRepository,
            IShowingSeatRepository showingSeatRepository,
            IShowingRepository showingRepository,
            ISeatRepository seatRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            IUserRepository userRepository,
            ISeatLockService seatLockService,
            ISeatHubService seatHubService,
            IConnectionMultiplexer redis,
            IBookingSessionRepository bookingSessionRepository)
        {
            _bookingRepository = bookingRepository;
            _bookingDetailRepository = bookingDetailRepository;
            _showingSeatRepository = showingSeatRepository;
            _showingRepository = showingRepository;
            _seatRepository = seatRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _userRepository = userRepository;
            _seatLockService = seatLockService;
            _seatHubService = seatHubService;
            _redis = redis;
            _bookingSessionRepository = bookingSessionRepository;
        }

        public async Task<Result<Guid>> Handle(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            List<Guid> seatIds = new();
            bool redisSuccess = false;

            // 1. Try to resolve user locked seats from Redis (SMEMBERS)
            try
            {
                var db = _redis.GetDatabase();
                var userKey = $"MovieApp:seat:user:{request.ShowingId}:{request.UserId}";
                var members = await db.SetMembersAsync(userKey);
                if (members != null && members.Length > 0)
                {
                    foreach (var member in members)
                    {
                        if (Guid.TryParse(member.ToString(), out var seatIdGuid))
                        {
                            seatIds.Add(seatIdGuid);
                        }
                    }
                    redisSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Failed to get user seats from Redis: {ex.Message}. Falling back to DB.");
            }

            // 2. Fallback to DB if Redis failed or returned no seats
            if (!redisSuccess || !seatIds.Any())
            {
                var session = await _bookingSessionRepository.GetSessionByShowingAndStatusAsync(
                    request.ShowingId,
                    request.UserId,
                    "select",
                    cancellationToken);

                if (session != null && session.BookingSessionSeats != null)
                {
                    seatIds = session.BookingSessionSeats.Select(s => s.SeatId).ToList();
                }
            }

            if (!seatIds.Any())
            {
                return Result.Failure<Guid>(new Error("Booking.NoSeatsSelected", "Không tìm thấy thông tin ghế đang giữ của bạn hoặc thời gian giữ ghế đã hết hạn."));
            }

            // ==================== READ & VERIFY OPERATIONS (OUTSIDE TRANSACTION) ====================

            // Retrieve existing showing seat records (booked or active locks)
            var existingShowingSeats = await _showingSeatRepository.GetShowingSeatsForBooking(
                request.ShowingId,
                seatIds,
                cancellationToken);

            var existingShowingSeatsMap = existingShowingSeats.ToDictionary(ss => ss.SeatId);

            // Verify in bookingSeats: if a seat is already booked, it won't be booked. Only satisfied seats are booked.
            var validSeatIds = new List<Guid>();
            foreach (var seatId in seatIds)
            {
                if (!existingShowingSeatsMap.TryGetValue(seatId, out var existingSeat) || existingSeat.Status != ShowingSeatStatus.Booked)
                {
                    validSeatIds.Add(seatId);
                }
            }

            if (!validSeatIds.Any())
            {
                return Result.Failure<Guid>(new Error("Booking.AllSeatsAlreadyBooked", "Tất cả các ghế bạn chọn đã được đặt bởi người khác."));
            }

            // Fetch physical seat configurations (to calculate price for available seats)
            var physicalSeats = await _seatRepository.GetSeatsWithTypesAsync(
                validSeatIds,
                cancellationToken);

            if (physicalSeats.Count != validSeatIds.Count)
            {
                return Result.Failure<Guid>(new Error("Booking.SeatsNotFound", "Một hoặc nhiều ghế được chọn không tồn tại trong hệ thống."));
            }

            // Fetch showing details
            var showing = await _showingRepository.GetByIdAsync(request.ShowingId, cancellationToken);
            if (showing == null)
            {
                return Result.Failure<Guid>(new Error("Booking.ShowingNotFound", "Suất chiếu không tồn tại."));
            }

            // Fetch user via repository instead of UserManager
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<Guid>(new Error("Booking.UserNotFound", "Không tìm thấy thông tin người dùng."));
            }

            var physicalSeatsMap = physicalSeats.ToDictionary(s => s.Id);
            var bookingId = Guid.NewGuid();
            decimal totalAmount = 0;

            // Calculate totalAmount based on available seats
            foreach (var seatId in validSeatIds)
            {
                var seat = physicalSeatsMap[seatId];
                totalAmount += showing.Price + (seat.SeatType?.Surcharge ?? 0);
            }

            // Validate user points
            if (user.Points < (double)totalAmount)
            {
                return Result.Failure<Guid>(new Error("Booking.InsufficientPoints", "Số dư điểm không đủ để thực hiện thanh toán."));
            }

            // ==================== WRITE & MODIFY OPERATIONS (INSIDE TRANSACTION) ====================

            // Begin transaction using UnitOfWork
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Create and save Booking to tracker (Parent entity first)
                var booking = Booking.Create(bookingId, totalAmount, StatusBooking.Confirmed, request.UserId, request.ShowingId);
                await _bookingRepository.AddAsync(booking, cancellationToken);

                // Create and save ShowingSeats + BookingDetails (Child entities next)
                foreach (var seatId in validSeatIds)
                {
                    var seat = physicalSeatsMap[seatId];
                    decimal seatPrice = showing.Price + (seat.SeatType?.Surcharge ?? 0);

                    // Create showing seat record
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

                    // Create booking detail record linked to the showing seat
                    var bookingDetail = BookingDetail.Create(
                        Guid.NewGuid(),
                        seatPrice,
                        BookingDetailStatus.Active,
                        bookingId,
                        newShowingSeat.Id);

                    await _bookingDetailRepository.AddAsync(bookingDetail, cancellationToken);
                }

                // Deduct points and update user using repository Update
                user.Points -= (double)totalAmount;
                _userRepository.Update(user);

                // Clear session and update ZSET in Redis/DB using the original seatIds (to unlock everything held)
                await _seatLockService.ClearSessionAfterBookingAsync(
                    request.ShowingId,
                    seatIds.Select(id => id.ToString()).ToList(),
                    request.UserId.ToString()
                );

                // Save all changes to database
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                // Notify all clients that validSeatIds are now booked
                try
                {
                    await _seatHubService.NotifySeatsBookedAsync(request.ShowingId, validSeatIds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR Error] Failed to broadcast SeatsBooked: {ex.Message}");
                }

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
