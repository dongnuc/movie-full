using System;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.AddTheaterMember;
using Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.RemoveTheaterMember;
using Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.UpdateTheaterMember;
using Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Queries.GetTheaterMembers;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.Theater
{
    public sealed record AddTheaterMemberRequest(string Username, Guid RoleId);
    public sealed record UpdateTheaterMemberRequest(Guid RoleId);

    public class MemberTheaterApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/theater/members";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("MemberTheater")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - Theater Members");

            group.MapGet("/", GetMembersAsync)
                .WithName("GetTheaterMembers")
                .WithSummary("Lấy danh sách nhân viên rạp (phân trang, lọc theo JWT token)")
                .Produces<PagedResult<TheaterMemberResponse>>(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Staff, ActionType.Read))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", AddMemberAsync)
                .WithName("AddTheaterMember")
                .WithSummary("Thêm nhân viên mới vào rạp bằng username và vai trò")
                .Produces(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Staff, ActionType.Create))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateMemberAsync)
                .WithName("UpdateTheaterMember")
                .WithSummary("Cập nhật vai trò nhân viên rạp")
                .Produces(StatusCodes.Status204NoContent)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Staff, ActionType.Update))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", RemoveMemberAsync)
                .WithName("RemoveTheaterMember")
                .WithSummary("Gỡ nhân viên khỏi rạp và trả về vai trò USER mặc định")
                .Produces(StatusCodes.Status204NoContent)
                .AddEndpointFilter(new BinaryAuthorizationFilter(PermissionConstants.Staff, ActionType.Delete))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetMembersAsync(
            ISender sender,
            HttpContext context,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null || userTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            var query = new GetTheaterMembersQuery(userTheaterId.Value, search, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> AddMemberAsync(
            ISender sender,
            HttpContext context,
            [FromBody] AddTheaterMemberRequest request)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null || userTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            var command = new AddTheaterMemberCommand(userTheaterId.Value, request.Username, request.RoleId);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok()
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateMemberAsync(
            ISender sender,
            HttpContext context,
            Guid id,
            [FromBody] UpdateTheaterMemberRequest request)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null || userTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            var command = new UpdateTheaterMemberCommand(userTheaterId.Value, id, request.RoleId);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> RemoveMemberAsync(
            ISender sender,
            HttpContext context,
            Guid id)
        {
            var userTheaterId = GetTheaterIdFromUser(context);
            if (userTheaterId == null || userTheaterId == Guid.Empty)
            {
                return Results.BadRequest(new { detail = "TheaterId not found in token claims." });
            }

            var command = new RemoveTheaterMemberCommand(userTheaterId.Value, id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
