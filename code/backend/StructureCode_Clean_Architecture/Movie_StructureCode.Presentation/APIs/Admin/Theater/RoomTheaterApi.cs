using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.CreateRoom;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.DeleteRoom;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.UpdateRoom;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.CheckSeatsExist;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetAllRooms;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomById;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomLayout;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomsByTheater;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.Theater
{
    public sealed record CreateRoomRequest(string Name, int TotalRow, int TotalCol, bool IsActive);
    public sealed record UpdateRoomRequest(string Name, int TotalRow, int TotalCol, bool IsActive);

    public class RoomTheaterApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/rooms";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("RoomTheater")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - Theater Rooms");

            group.MapGet("/", GetAllRoomsAsync)
                .WithName("GetAllRooms")
                .WithSummary("Lấy danh sách phòng chiếu (phân trang, lọc theo rạp hoặc JWT token)")
                .Produces<PagedResult<RoomResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetRoomByIdAsync)
                .WithName("GetRoomById")
                .WithSummary("Lấy thông tin chi tiết phòng chiếu theo ID")
                .Produces<RoomDetailResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}/layout", GetRoomLayoutAsync)
                .WithName("GetRoomLayout")
                .WithSummary("Lấy sơ đồ chỗ ngồi của phòng chiếu theo ID")
                .Produces<RoomLayoutResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}/seats/exists", CheckSeatsExistAsync)
                .WithName("CheckSeatsExist")
                .WithSummary("Kiểm tra xem phòng chiếu có tồn tại ghế hay không")
                .Produces<bool>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateRoomAsync)
                .WithName("CreateRoom")
                .WithSummary("Tạo phòng chiếu mới (sử dụng TheaterId từ request hoặc JWT token)")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateRoomAsync)
                .WithName("UpdateRoom")
                .WithSummary("Cập nhật thông tin phòng chiếu (sử dụng TheaterId từ request hoặc JWT token)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteRoomAsync)
                .WithName("DeleteRoom")
                .WithSummary("Xóa phòng chiếu theo ID")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/theater/{theaterId:guid}", GetRoomsByTheaterAsync)
                .WithName("GetRoomsByTheater")
                .WithSummary("Lấy danh sách phòng chiếu thuộc rạp cụ thể")
                .Produces<IEnumerable<RoomMinimalResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/theater", GetRoomsByMyTheaterAsync)
                .WithName("GetRoomsByMyTheater")
                .WithSummary("Lấy danh sách phòng chiếu thuộc rạp của người dùng hiện tại (từ JWT token)")
                .Produces<IEnumerable<RoomMinimalResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetAllRoomsAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            var finalTheaterId = userTheaterId;

            if (finalTheaterId == null || finalTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId is required." });
            }

            var query = new GetAllRoomsQuery(search, isActive, finalTheaterId.Value, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetRoomsByTheaterAsync(
            ISender sender,
            HttpContext context,
            Guid theaterId)
        {   
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null || userTheaterId != theaterId)
            {
                return Results.BadRequest(new { detail = "TheaterId is not match with token." });
            }

            var query = new GetRoomsByTheaterQuery(theaterId);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetRoomsByMyTheaterAsync(
            ISender sender,
            HttpContext context)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            var query = new GetRoomsByTheaterQuery(userTheaterId.Value);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetRoomByIdAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetRoomByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetRoomLayoutAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetRoomLayoutQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CheckSeatsExistAsync(
            ISender sender,
            Guid id)
        {
            var query = new CheckSeatsExistQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateRoomAsync(
            ISender sender,
            HttpContext context,
            [FromBody] CreateRoomRequest request)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            var finalTheaterId = userTheaterId;

            if (finalTheaterId == null || finalTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId is required." });
            }

            var command = new CreateRoomCommand(
                request.Name,
                request.TotalRow,
                request.TotalCol,
                finalTheaterId.Value,
                request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo phòng chiếu thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateRoomAsync(
            ISender sender,
            HttpContext context,
            Guid id,
            [FromBody] UpdateRoomRequest request)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            var finalTheaterId = userTheaterId;

            if (finalTheaterId == null || finalTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId is required." });
            }

            var command = new UpdateRoomCommand(
                id,
                request.Name,
                request.TotalRow,
                request.TotalCol,
                finalTheaterId.Value,
                request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteRoomAsync(
            ISender sender,
            Guid id)
        {
            var command = new DeleteRoomCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
