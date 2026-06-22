using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.CreateShowing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.UpdateShowing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingDetail;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingInRoom;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingsByTheater;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;
using Movie_StructureCode.Application.Features.UseCase.Admin.Bookings.Queries.GetBookingsByShowing;

namespace Movie_StructureCode.Presentation.APIs.Admin.Theater
{
    public sealed record CreateShowingRequest(
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid MovieId,
        Guid RoomId,
        bool IsActive);

    public sealed record UpdateShowingRequest(
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid RoomId,
        bool IsActive);

    public class ShowingTheaterApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/showings";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("ShowingTheater")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - Theater Showings");

            group.MapGet("/theater/{theaterId:guid}", GetShowingsByTheaterAsync)
                .WithName("GetShowingsByTheater")
                .WithSummary("Lấy toàn bộ lịch chiếu thuộc rạp cụ thể (gom theo movie, phân trang)")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Read))
                .Produces<PagedResult<ShowingGroupedByMovie>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/theater", GetShowingsByMyTheaterAsync)
                .WithName("GetShowingsByMyTheater")
                .WithSummary("Lấy toàn bộ lịch chiếu thuộc rạp của người dùng hiện tại (từ JWT token)")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Read))
                .Produces<PagedResult<ShowingGroupedByMovie>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/room/{theaterId:guid}", GetShowingsGroupedByTheaterRoomAsync)
                .WithName("GetShowingsGroupedByTheaterRoom")
                .WithSummary("Lấy toàn bộ lịch chiếu gom theo room cho rạp cụ thể")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Read))
                .Produces<PagedResult<ShowingGroupedRoom>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/room", GetShowingsGroupedByRoomAsync)
                .WithName("GetShowingsGroupedByRoom")
                .WithSummary("Lấy toàn bộ lịch chiếu gom theo room cho rạp hiện tại")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Read))
                .Produces<PagedResult<ShowingGroupedRoom>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetShowingDetailAsync)
                .WithName("GetShowingDetail")
                .WithSummary("Lấy thông tin chi tiết lịch chiếu theo ID")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Read))
                .Produces<ShowingDetailResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{showingId:guid}/bookings", GetBookingsByShowingAsync)
                .WithName("GetBookingsByShowing")
                .WithSummary("Lấy danh sách vé/đặt chỗ của một lịch chiếu thuộc rạp")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Ticket, ActionType.Read))
                .Produces<List<ShowingBookingResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateShowingAsync)
                .WithName("CreateShowing")
                .WithSummary("Tạo lịch chiếu mới")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Create))
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateShowingAsync)
                .WithName("UpdateShowing")
                .WithSummary("Cập nhật thông tin lịch chiếu")
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Showing, ActionType.Update))
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetShowingsByMyTheaterAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] string? dateStart,
            [FromQuery] Guid? roomId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            return await ExecuteGetShowingsAsync(sender, userTheaterId.Value, dateStart, roomId, pageIndex, pageSize, isActive);
        }

        private static async Task<IResult> GetShowingsByTheaterAsync(
            ISender sender,
            Guid theaterId,
            [FromQuery] string? dateStart,
            [FromQuery] Guid? roomId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            return await ExecuteGetShowingsAsync(sender, theaterId, dateStart, roomId, pageIndex, pageSize, isActive);
        }

        private static async Task<IResult> ExecuteGetShowingsAsync(
            ISender sender,
            Guid theaterId,
            string? dateStart,
            Guid? roomId,
            int pageIndex,
            int pageSize,
            bool? isActive)
        {
            DateOnly? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(dateStart))
            {
                if (!DateOnly.TryParseExact(dateStart, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" }, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out var dateParsed))
                {
                    return Results.BadRequest(new { message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng yyyy-MM-dd hoặc dd/MM/yyyy" });
                }
                parsedDate = dateParsed;
            }

            var query = new GetShowingsByTheaterQuery(theaterId, parsedDate, roomId, pageIndex, pageSize, isActive);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetShowingsGroupedByRoomAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] string? dateStart,
            [FromQuery] Guid? roomId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            return await ExecuteGetShowingsGroupedByRoomAsync(sender, userTheaterId.Value, dateStart, roomId, pageIndex, pageSize, isActive);
        }

        private static async Task<IResult> GetShowingsGroupedByTheaterRoomAsync(
            ISender sender,
            Guid theaterId,
            [FromQuery] string? dateStart,
            [FromQuery] Guid? roomId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            return await ExecuteGetShowingsGroupedByRoomAsync(sender, theaterId, dateStart, roomId, pageIndex, pageSize, isActive);
        }

        private static async Task<IResult> ExecuteGetShowingsGroupedByRoomAsync(
            ISender sender,
            Guid theaterId,
            string? dateStart,
            Guid? roomId,
            int pageIndex,
            int pageSize,
            bool? isActive)
        {
            DateOnly? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(dateStart))
            {
                if (!DateOnly.TryParseExact(dateStart, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" }, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out var dateParsed))
                {
                    return Results.BadRequest(new { message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng yyyy-MM-dd hoặc dd/MM/yyyy" });
                }
                parsedDate = dateParsed;
            }

            var query = new GetShowingInRoomQuery(theaterId, parsedDate, roomId, pageIndex, pageSize, isActive);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetShowingDetailAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetShowingDetailQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateShowingAsync(
            ISender sender,
            [FromBody] CreateShowingRequest request)
        {
            var command = new CreateShowingCommand(
                request.TimeStart,
                request.Price,
                request.MaxTicketsPerUser,
                request.MovieId,
                request.RoomId,
                request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo lịch chiếu thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateShowingAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateShowingRequest request)
        {
            var command = new UpdateShowingCommand(
                id,
                request.TimeStart,
                request.Price,
                request.MaxTicketsPerUser,
                request.RoomId,
                request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetBookingsByShowingAsync(
            ISender sender,
            HttpContext context,
            Guid showingId)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            var query = new GetBookingsByShowingQuery(showingId, userTheaterId);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
