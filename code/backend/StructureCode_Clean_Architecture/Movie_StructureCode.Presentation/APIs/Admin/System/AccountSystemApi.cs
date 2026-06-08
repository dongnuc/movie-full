using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.CreateAccount;
using Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.ToggleAccountStatus;
using Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.UpdateAccount;
using Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Queries.GetAccounts;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateAccountRequest(
        string Username,
        string Email,
        string Password,
        string? RoleId,
        List<string>? TheaterIds
    );

    public sealed record UpdateAccountRequest(
        string Username,
        string Email,
        string RoleId,
        List<string>? TheaterIds
    );

    public class AccountSystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/accounts";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("AccountSystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                // .RequireAuthorization("SystemAdmin") // Only system admins can access user accounts
                .WithTags("Admin - System Accounts")
                .AllowAnonymous();

            group.MapGet("/", GetAccountsAsync)
                .WithName("GetAccounts")
                .WithSummary("Lấy danh sách tài khoản (phân trang, lọc theo vai trò)")
                .Produces<PagedResult<AccountResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateAccountAsync)
                .WithName("CreateAccount")
                .WithSummary("Tạo tài khoản người dùng mới (phân quyền, rạp nếu là TheaterOwner)")
                .Produces<Guid>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateAccountAsync)
                .WithName("UpdateAccount")
                .WithSummary("Cập nhật thông tin tài khoản (quyền, rạp liên kết)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", ToggleAccountStatusAsync)
                .WithName("ToggleAccountStatus")
                .WithSummary("Khóa/Mở khóa tài khoản (Soft-delete/Deactivate)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetAccountsAsync(
            ISender sender,
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAccountsQuery(search, role, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateAccountAsync(
            ISender sender,
            [FromBody] CreateAccountRequest request)
        {
            var theaterGuids = new List<Guid>();
            if (request.TheaterIds != null)
            {
                foreach (var tId in request.TheaterIds)
                {
                    if (Guid.TryParse(tId, out var g))
                    {
                        theaterGuids.Add(g);
                    }
                    else
                    {
                        return Results.BadRequest(new { detail = $"TheaterId '{tId}' is not a valid Guid." });
                    }
                }
            }

            var command = new CreateAccountCommand(
                request.Username,
                request.Email,
                request.Password,
                request.RoleId,
                theaterGuids
            );
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateAccountAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleId) || !Guid.TryParse(request.RoleId, out var roleIdGuid))
            {
                return Results.BadRequest(new { detail = "RoleId must be a valid Guid." });
            }

            var theaterGuids = new List<Guid>();
            if (request.TheaterIds != null)
            {
                foreach (var tId in request.TheaterIds)
                {
                    if (Guid.TryParse(tId, out var g))
                    {
                        theaterGuids.Add(g);
                    }
                    else
                    {
                        return Results.BadRequest(new { detail = $"TheaterId '{tId}' is not a valid Guid." });
                    }
                }
            }

            var command = new UpdateAccountCommand(
                id,
                request.Username,
                request.Email,
                roleIdGuid,
                theaterGuids
            );
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> ToggleAccountStatusAsync(
            ISender sender,
            Guid id)
        {
            var command = new ToggleAccountStatusCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
