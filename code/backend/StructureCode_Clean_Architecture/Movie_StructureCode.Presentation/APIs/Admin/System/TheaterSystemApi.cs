using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.CreateTheater;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.DeleteTheater;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.UpdateTheater;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetAllTheaters;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetTheaterById;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateTheaterRequest(string Name, string? Location, bool IsActive);
    public sealed record UpdateTheaterRequest(string Name, string? Location, bool IsActive);


    public class TheaterSystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/theaters";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("TheaterSystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - System Theaters")
                .AllowAnonymous();

            group.MapGet("/", GetAllTheatersAsync)
                .WithName("GetAllTheaters")
                .WithSummary("Lấy danh sách rạp chiếu phim (phân trang, tìm kiếm, lọc)")
                .Produces<PagedResult<TheaterResponse>>(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Theater, ActionType.Read))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetTheaterByIdAsync)
                .WithName("GetTheaterById")
                .WithSummary("Lấy thông tin rạp chiếu phim theo ID")
                .Produces<TheaterResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateTheaterAsync)
                .WithName("CreateTheater")
                .WithSummary("Tạo rạp chiếu phim mới")
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateTheaterAsync)
                .WithName("UpdateTheater")
                .WithSummary("Cập nhật thông tin rạp chiếu phim")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteTheaterAsync)
                .WithName("DeleteTheater")
                .WithSummary("Xóa rạp chiếu phim theo ID")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetAllTheatersAsync(
            ISender sender,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllTheatersQuery(search, isActive, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetTheaterByIdAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetTheaterByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateTheaterAsync(
            ISender sender,
            [FromBody] CreateTheaterRequest request)
        {
            var command = new CreateTheaterCommand(request.Name, request.Location, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo rạp chiếu phim thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateTheaterAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateTheaterRequest request)
        {
            var command = new UpdateTheaterCommand(id, request.Name, request.Location, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteTheaterAsync(
            ISender sender,
            Guid id)
        {
            var command = new DeleteTheaterCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }


}
