using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetTheaterRoles;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.Theater
{
    public class RoleTheaterApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/theater/roles";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("RoleTheater")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - Theater Roles");

            group.MapGet("/", GetTheaterRolesAsync)
                .WithName("GetTheaterRoles")
                .WithSummary("Lấy danh sách các vai trò của rạp chiếu phim (chứa THEATER)")
                .Produces<List<RoleResponse>>(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Staff, ActionType.Read))
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetTheaterRolesAsync(ISender sender)
        {
            var query = new GetTheaterRolesQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
