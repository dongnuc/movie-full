using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Bookings.Commands.CreateBooking;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class BookingUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/bookings";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("User - Booking")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .RequireAuthorization();  // Tất cả booking endpoint phải authenticated

            group.MapPost("/", CreateBookingAsync)
                .WithName("CreateBooking")
                .WithSummary("Tạo đặt vé và gửi email xác nhận cho người dùng")
                .Produces<Guid>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> CreateBookingAsync(
            ISender sender,
            HttpContext context,
            [FromBody] CreateBookingRequest request)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var command = new CreateBookingCommand(request.SeatIds, request.ShowingId, userId.Value);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static Guid? GetUserIdFromContext(HttpContext context)
        {
            var claimValue = context.User.FindFirst("sub")?.Value
                             ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claimValue, out var guid) ? guid : null;
        }
    }

    public sealed record CreateBookingRequest(
        List<Guid> SeatIds,
        Guid ShowingId
    );
}
