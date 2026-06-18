using System;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Accounts.Queries.GetUserPoints;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class AccountUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/accounts";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("User - Accounts")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .RequireAuthorization();

            group.MapGet("/points", GetUserPointsAsync)
                .WithName("GetUserPoints")
                .WithSummary("Lấy số điểm tích lũy của người dùng hiện tại")
                .Produces<UserPointsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest);
        }

        private static async Task<IResult> GetUserPointsAsync(
            ISender sender,
            HttpContext context)
        {
            var userId = GetUserIdFromContext(context);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var query = new GetUserPointsQuery(userId.Value);
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
