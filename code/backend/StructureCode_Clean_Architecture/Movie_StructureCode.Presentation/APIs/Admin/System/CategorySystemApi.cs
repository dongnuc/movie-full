using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.CreateCategory;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.UpdateCategory;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.DeleteCategory;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Queries.GetAllCategories;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Queries.GetCategoryById;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateCategoryRequest(string Name, bool IsActive);
    public sealed record UpdateCategoryRequest(string Name, bool IsActive);

    public class CategorySystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/categories";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("CategorySystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - System Categories");

            group.MapGet("/", GetAllCategoriesAsync)
                .WithName("GetAllCategories")
                .WithSummary("Lấy danh sách thể loại phim (phân trang, lọc theo ID)")
                .Produces<PagedResult<CategoryResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetCategoryByIdAsync)
                .WithName("GetCategoryById")
                .WithSummary("Lấy thông tin thể loại phim theo ID")
                .Produces<CategoryResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateCategoryAsync)
                .WithName("CreateCategory")
                .WithSummary("Tạo thể loại phim mới")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateCategoryAsync)
                .WithName("UpdateCategory")
                .WithSummary("Cập nhật thông tin thể loại phim")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteCategoryAsync)
                .WithName("DeleteCategory")
                .WithSummary("Xóa thể loại phim theo ID")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetAllCategoriesAsync(
            ISender sender,
            [FromQuery] Guid? id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllCategoriesQuery(id, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetCategoryByIdAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetCategoryByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateCategoryAsync(
            ISender sender,
            [FromBody] CreateCategoryRequest request)
        {
            var command = new CreateCategoryCommand(request.Name, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { message = "Tạo thể loại phim thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateCategoryAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateCategoryRequest request)
        {
            var command = new UpdateCategoryCommand(id, request.Name, request.IsActive);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteCategoryAsync(
            ISender sender,
            Guid id)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }
    }
}
