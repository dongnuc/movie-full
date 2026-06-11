using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Abstractions.Services.Email;
using Movie_StructureCode.Contract.Emails;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Events
{
    internal sealed class BookingConfirmedNotificationHandler : INotificationHandler<BookingConfirmedNotification>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEmailServices _emailServices;

        public BookingConfirmedNotificationHandler(
            IBookingRepository bookingRepository,
            IEmailServices emailServices)
        {
            _bookingRepository = bookingRepository;
            _emailServices = emailServices;
        }

        public async Task Handle(BookingConfirmedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve the booking with full detail graph
                var booking = await _bookingRepository.GetBookingWithDetailsForEmailAsync(notification.BookingId, cancellationToken);
                if (booking == null)
                {
                    Console.WriteLine($"[Email Error] Booking with ID {notification.BookingId} was not found.");
                    return;
                }

                var userEmail = booking.AppUser?.Email;
                if (string.IsNullOrEmpty(userEmail))
                {
                    Console.WriteLine($"[Email Error] User email for booking {booking.Id} is empty.");
                    return;
                }

                // Extract metadata
                var movieTitle = booking.Showing?.Movie?.Title ?? "N/A";
                var duration = booking.Showing?.Movie?.Duration ?? 0;
                var showingTime = booking.Showing?.TimeStart.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                var roomName = booking.Showing?.Room?.Name ?? "N/A";
                var theaterName = booking.Showing?.Room?.Theater?.Name ?? "N/A";
                var theaterAddress = booking.Showing?.Room?.Theater?.Location ?? "N/A";

                // Extract seat names and count
                var seatNamesList = new List<string>();
                if (booking.BookingDetails != null && booking.BookingDetails.Any())
                {
                    foreach (var detail in booking.BookingDetails)
                    {
                        var seat = detail.ShowingSeat?.Seat;
                        if (seat != null)
                        {
                            seatNamesList.Add($"{seat.RowName}{seat.SeatNumber}");
                        }
                    }
                }
                var seatCount = seatNamesList.Count;
                var seatNamesString = seatCount > 0 ? string.Join(", ", seatNamesList) : "N/A";

                // Construct Premium Netflix-themed HTML body
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; background-color: #141414; color: #ffffff; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #181818; border-radius: 6px; overflow: hidden; border: 1px solid #282828; box-shadow: 0 10px 30px rgba(0,0,0,0.5); }}
                            .header {{ background-color: #141414; padding: 25px; text-align: center; border-bottom: 3px solid #e50914; }}
                            .header h2 {{ color: #e50914; margin: 0; font-size: 22px; font-weight: 800; letter-spacing: 2px; text-transform: uppercase; }}
                            .content {{ padding: 30px; }}
                            .movie-title {{ font-size: 24px; font-weight: 900; color: #ffffff; margin-bottom: 20px; border-left: 4px solid #e50914; padding-left: 10px; }}
                            .details-grid {{ display: table; width: 100%; margin-bottom: 30px; }}
                            .details-row {{ display: table-row; }}
                            .details-cell {{ display: table-cell; padding: 8px 0; font-size: 14px; border-bottom: 1px solid #222; }}
                            .label {{ color: #808080; width: 30%; font-weight: bold; }}
                            .value {{ color: #ffffff; font-weight: 500; }}
                            .qr-card {{ margin-top: 30px; padding: 25px; background-color: #202020; border: 1px dashed #e50914; border-radius: 4px; text-align: center; }}
                            .qr-label {{ font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: #808080; margin-bottom: 8px; font-weight: bold; }}
                            .qr-value {{ font-family: monospace; font-size: 18px; font-weight: bold; color: #ffffff; letter-spacing: 2px; }}
                            .qr-hint {{ font-size: 11px; color: #666; margin-top: 8px; }}
                            .footer {{ background-color: #141414; padding: 20px; text-align: center; font-size: 11px; color: #666; border-top: 1px solid #282828; }}
                            .footer p {{ margin: 4px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đặt Vé Thành Công</h2>
                            </div>
                            
                            <div class='content'>
                                <div class='movie-title'>{movieTitle}</div>
                                
                                <div class='details-grid'>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Rạp Chiếu:</div>
                                        <div class='details-cell value'>{theaterName}</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Địa Chỉ:</div>
                                        <div class='details-cell value'>{theaterAddress}</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Phòng Chiếu:</div>
                                        <div class='details-cell value'>{roomName}</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Suất Chiếu:</div>
                                        <div class='details-cell value'>{showingTime}</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Thời Lượng:</div>
                                        <div class='details-cell value'>{duration} phút</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Số Lượng Ghế:</div>
                                        <div class='details-cell value'>{seatCount}</div>
                                    </div>
                                    <div class='details-row'>
                                        <div class='details-cell label'>Vị Trí Ghế:</div>
                                        <div class='details-cell value'>{seatNamesString}</div>
                                    </div>
                                </div>

                                <div class='qr-card'>
                                    <div class='qr-label'>Mã Vé Nhận Tại Quầy</div>
                                    <div class='qr-value'>{booking.Id}</div>
                                    <div class='qr-hint'>Vui lòng xuất trình mã này tại quầy để nhận vé xem phim của bạn.</div>
                                </div>
                            </div>
                            
                            <div class='footer'>
                                <p>Cảm ơn bạn đã sử dụng dịch vụ Movie Booking Application.</p>
                                <p>Chúc bạn có một buổi xem phim vui vẻ!</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Send email
                var message = new Message(new[] { userEmail }, "Xác Nhận Đặt Vé Xem Phim Thành Công", htmlBody);
                await _emailServices.SendEmail(message);
            }
            catch (Exception ex)
            {
                // Catch SMTP or formatting exceptions so that the user's transaction/booking stays successful
                Console.WriteLine($"[Email Exception] Failed to send ticket confirmation for booking {notification.BookingId}: {ex.Message}");
            }
        }
    }
}
