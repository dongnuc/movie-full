using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.CreateSeatType;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.DeleteSeatType;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.UpdateSeatType;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetActiveSeatTypes;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetAllSeatTypes;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetSeatTypeById;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateSeatTypeRequest(string Name, decimal Surcharge, bool IsActive);
    public sealed record UpdateSeatTypeRequest(string Name, decimal Surcharge, bool IsActive);

    public class SeatTypeSystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/seattypes";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("SeatTypeSystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - System SeatTypes");

            group.MapGet("/active", GetActiveSeatTypesAsync)
                .WithName("GetActiveSeatTypes")
                .WithSummary("Lấy danh sách loại ghế đang hoạt động (chỉ gồm ID, tên, phụ phí)")
                .Produces<IEnumerable<SeatTypeActiveResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/", GetAllSeatTypesAsync)
                .WithName("GetAllSeatTypes")
                .WithSummary("Lấy danh sách tất cả loại ghế (phân trang, lọc theo ID)")
                .Produces<PagedResult<SeatTypeResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetSeatTypeByIdAsync)
                .WithName("GetSeatTypeById")
                .WithSummary("Lấy thông tin loại ghế theo ID")
                .Produces<SeatTypeResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateSeatTypeAsync)
                .WithName("CreateSeatType")
                .WithSummary("Tạo loại ghế mới")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateSeatTypeAsync)
                .WithName("UpdateSeatType")
                .WithSummary("Cập nhật thông tin loại ghế")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteSeatTypeAsync)
                .WithName("DeleteSeatType")
                .WithSummary("Xóa loại ghế theo ID")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetActiveSeatTypesAsync(ISender sender)
        {
            var query = new GetActiveSeatTypesQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetAllSeatTypesAsync(
            ISender sender,
            [FromQuery] Guid? id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllSeatTypesQuery(id, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetSeatTypeByIdAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetSeatTypeByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateSeatTypeAsync(
            ISender sender,
            [FromBody] CreateSeatTypeRequest request)
        {
            var command = new CreateSeatTypeCommand(request.Name, request.Surcharge, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo loại ghế thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateSeatTypeAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateSeatTypeRequest request)
        {
            var command = new UpdateSeatTypeCommand(id, request.Name, request.Surcharge, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteSeatTypeAsync(
            ISender sender,
            Guid id)
        {
            var command = new DeleteSeatTypeCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
