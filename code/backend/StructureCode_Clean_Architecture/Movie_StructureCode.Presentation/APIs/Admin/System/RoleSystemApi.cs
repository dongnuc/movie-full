using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.CreateRole;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.DeleteRole;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoleById;
using Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoles;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateRoleRequest(string Name);
    public sealed record UpdateRoleRequest(string Name);

    public class RoleSystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/roles";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("RoleSystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                // .RequireAuthorization("SystemAdmin") // Optional: only system admins can manage roles
                .WithTags("Admin - System Roles")
                .AllowAnonymous();

            group.MapGet("/", GetRolesAsync)
                .WithName("GetRoles")
                .WithSummary("Lấy danh sách các vai trò (Roles)")
                .Produces<List<RoleResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetRoleByIdAsync)
                .WithName("GetRoleById")
                .WithSummary("Lấy thông tin vai trò theo ID")
                .Produces<RoleResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateRoleAsync)
                .WithName("CreateRole")
                .WithSummary("Tạo vai trò mới")
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateRoleAsync)
                .WithName("UpdateRole")
                .WithSummary("Cập nhật thông tin vai trò")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteRoleAsync)
                .WithName("DeleteRole")
                .WithSummary("Xóa vai trò khỏi hệ thống")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetRolesAsync(ISender sender)
        {
            var query = new GetRolesQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetRoleByIdAsync(ISender sender, Guid id)
        {
            var query = new GetRoleByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateRoleAsync(
            ISender sender,
            [FromBody] CreateRoleRequest request)
        {
            var command = new CreateRoleCommand(request.Name);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateRoleAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateRoleRequest request)
        {
            var command = new UpdateRoleCommand(id, request.Name);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteRoleAsync(ISender sender, Guid id)
        {
            var command = new DeleteRoleCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
