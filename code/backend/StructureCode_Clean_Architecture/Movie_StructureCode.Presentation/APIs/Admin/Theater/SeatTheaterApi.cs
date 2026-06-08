using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.CreateSeatsInBulk;
using Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsTypeInBulk;
using Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsToCouple;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.Theater
{
    public sealed record CreateSeatsInBulkRequest(Guid RoomId);
    public sealed record UpdateSeatsTypeInBulkRequest(List<Guid> SeatIds, Guid SeatTypeId);
    public sealed record UpdateSeatsToCoupleRequest(List<Guid> SeatIds);

    public class SeatTheaterApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/seats";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("SeatTheater")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - Theater Seats");

            group.MapPost("/bulk", CreateSeatsInBulkAsync)
                .WithName("CreateSeatsInBulk")
                .WithSummary("Tạo sơ đồ ghế hàng loạt tự động cho phòng chiếu")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/seattype/bulk", UpdateSeatsTypeInBulkAsync)
                .WithName("UpdateSeatsTypeInBulk")
                .WithSummary("Cập nhật loại ghế hàng loạt cho danh sách ghế")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/seattype/couple", UpdateSeatsToCoupleAsync)
                .WithName("UpdateSeatsToCouple")
                .WithSummary("Cập nhật các ghế được chọn thành ghế Couple (yêu cầu số lượng chẵn, cùng hàng và liền kề nhau)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

        }

        private static async Task<IResult> CreateSeatsInBulkAsync(
            ISender sender,
            [FromBody] CreateSeatsInBulkRequest request)
        {
            var command = new CreateSeatsInBulkCommand(request.RoomId);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo sơ đồ ghế hàng loạt thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateSeatsTypeInBulkAsync(
            ISender sender,
            [FromBody] UpdateSeatsTypeInBulkRequest request)
        {
            var command = new UpdateSeatsTypeInBulkCommand(request.SeatIds, request.SeatTypeId);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateSeatsToCoupleAsync(
            ISender sender,
            [FromBody] UpdateSeatsToCoupleRequest request)
        {
            var command = new UpdateSeatsToCoupleCommand(request.SeatIds);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
