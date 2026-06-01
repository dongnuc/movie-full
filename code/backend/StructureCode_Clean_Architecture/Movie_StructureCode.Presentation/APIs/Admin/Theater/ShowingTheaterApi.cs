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
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingsByTheater;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

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
                .WithSummary("Lấy toàn bộ lịch chiếu thuộc rạp (gom theo movie, phân trang)")
                .Produces<PagedResult<ShowingGroupedByMovie>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetShowingDetailAsync)
                .WithName("GetShowingDetail")
                .WithSummary("Lấy thông tin chi tiết lịch chiếu theo ID")
                .Produces<ShowingDetailResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateShowingAsync)
                .WithName("CreateShowing")
                .WithSummary("Tạo lịch chiếu mới")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateShowingAsync)
                .WithName("UpdateShowing")
                .WithSummary("Cập nhật thông tin lịch chiếu")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetShowingsByTheaterAsync(
            ISender sender,
            Guid theaterId,
            [FromQuery] string? dateStart,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
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

            var query = new GetShowingsByTheaterQuery(theaterId, parsedDate, pageIndex, pageSize, isActive);
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
    }
}
