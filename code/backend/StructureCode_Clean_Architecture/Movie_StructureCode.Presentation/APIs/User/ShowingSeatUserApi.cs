using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetShowingSeatsLayout;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.HoldSeats;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.UnholdSeats;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetSessionTtl;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.MarkSessionAsPaying;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class ShowingSeatUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/showing-seats";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("ShowingSeatUser")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("User - Showing Seats");

            group.MapGet("/layout", GetShowingSeatsLayoutAsync)
                .WithName("GetShowingSeatsLayout")
                .WithSummary("Lấy sơ đồ chỗ ngồi kèm trạng thái và quyền sở hữu cho người dùng")
                .Produces<UserRoomLayoutResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/hold", HoldSeatsAsync)
                .RequireAuthorization()
                .WithName("HoldSeats")
                .WithSummary("Giữ ghế cho người dùng trong suất chiếu")
                .Produces<HoldSeatsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("/unhold", UnholdSeatsAsync)
                .RequireAuthorization()
                .WithName("UnholdSeats")
                .WithSummary("Hủy giữ ghế cho người dùng trong suất chiếu")
                .Produces<bool>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapGet("/session-ttl", GetSessionTtlAsync)
                .RequireAuthorization()
                .WithName("GetSessionTtl")
                .WithSummary("Lấy thời gian còn lại (TTL) của session giữ ghế dựa trên showingId")
                .Produces<GetSessionTtlResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/paying", MarkSessionAsPayingAsync)
                .RequireAuthorization()
                .WithName("MarkSessionAsPaying")
                .WithSummary("Đánh dấu trạng thái session thành thanh toán và gia hạn TTL")
                .Produces<MarkSessionAsPayingResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);
        }

        private static async Task<IResult> GetShowingSeatsLayoutAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] Guid showingId,
            [FromQuery] Guid? roomId = null)
        {
            var userId = GetUserIdFromContext(context);
            var query = new GetShowingSeatsLayoutQuery(showingId, roomId, userId);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> HoldSeatsAsync(
            ISender sender,
            HttpContext context,
            [FromBody] HoldSeatsRequest request)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var command = new HoldSeatsCommand(request.ShowingId, request.SeatIds, userId.Value);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> UnholdSeatsAsync(
            ISender sender,
            HttpContext context,
            [FromBody] UnholdSeatsRequest request)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var command = new UnholdSeatsCommand(request.ShowingId, request.SeatIds, userId.Value);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetSessionTtlAsync(
            [FromQuery] Guid showingId,
            HttpContext context,
            ISender sender)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var query = new GetSessionTtlQuery(showingId, userId.Value);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> MarkSessionAsPayingAsync(
            [FromBody] MarkSessionAsPayingRequest request,
            HttpContext context,
            ISender sender)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var command = new MarkSessionAsPayingCommand(request.ShowingId, userId.Value);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new MarkSessionAsPayingResponse(result.Value))
                : HandlerFailure(result);
        }

        private static Guid? GetUserIdFromContext(HttpContext context)
        {
            var claimValue = context.User.FindFirst("sub")?.Value 
                             ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claimValue, out var guid) ? guid : null;
        }
    }

    public sealed record HoldSeatsRequest(
        Guid ShowingId,
        List<string> SeatIds
    );

    public sealed record UnholdSeatsRequest(
        Guid ShowingId,
        List<string> SeatIds
    );

    public sealed record MarkSessionAsPayingRequest(
        Guid ShowingId
    );

    public sealed record MarkSessionAsPayingResponse(
        int? RemainingSeconds
    );
}
