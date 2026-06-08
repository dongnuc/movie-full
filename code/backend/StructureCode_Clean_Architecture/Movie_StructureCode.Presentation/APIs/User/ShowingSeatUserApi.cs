using System;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetShowingSeatsLayout;
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
        }

        private static async Task<IResult> GetShowingSeatsLayoutAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] Guid showingId,
            [FromQuery] Guid roomId)
        {
            var userId = GetUserIdFromContext(context);
            var query = new GetShowingSeatsLayoutQuery(showingId, roomId, userId);
            var result = await sender.Send(query);

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
}
