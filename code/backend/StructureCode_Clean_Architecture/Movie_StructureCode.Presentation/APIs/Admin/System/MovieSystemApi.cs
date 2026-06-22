using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.CreateMovie;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.DeleteMovie;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UpdateMovie;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UploadMovieImages;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetAllMoviesAdmin;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetMovieDetailAdmin;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.Admin.System
{
    public sealed record CreateMovieRequest(
        string Title,
        string? Description,
        string? Image,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        bool IsActive,
        bool IsHomePage = false);

    public sealed record UpdateMovieRequest(
        string Title,
        string? Description,
        string? Image,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        bool IsActive,
        bool IsHomePage = false,
        bool ClearBanner = false,
        bool ClearPoster = false);

    public class MovieSystemApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/admin/movies";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("MovieSystem")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("Admin - System Movies")
                .AllowAnonymous();

            group.MapGet("/", GetAllMoviesAsync)
                .WithName("GetAllMovies")
                .WithSummary("Lấy danh sách phim (phân trang, tìm kiếm, lọc)")
                .Produces<PagedResult<MovieResponse>>(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Read)
                ))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetMovieByIdAsync)
                .WithName("GetMovieById")
                .WithSummary("Lấy thông tin chi tiết phim theo ID")
                .Produces<MovieDetailResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Read)
                ))
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/", CreateMovieAsync)
                .WithName("CreateMovie")
                .WithSummary("Tạo phim mới")
                .Produces(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Create)
                ))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateMovieAsync)
                .WithName("UpdateMovie")
                .WithSummary("Cập nhật thông tin phim")
                .Produces(StatusCodes.Status204NoContent)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Update)
                ))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteMovieAsync)
                .WithName("DeleteMovie")
                .WithSummary("Xóa phim theo ID")
                .Produces(StatusCodes.Status204NoContent)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Delete)
                ))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{id:guid}/upload-images", UploadMovieImagesAsync)
                .WithName("UploadMovieImages")
                .WithSummary("Upload ảnh banner và poster cho phim (Validate kích thước/hướng ảnh)")
                .DisableAntiforgery()
                .Produces(StatusCodes.Status200OK)
                .AddEndpointFilter(new BinaryAuthorizationFilter(
                    new PermissionRequirement(PermissionConstants.Movie, ActionType.Update)
                ))
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }

        private static async Task<IResult> GetAllMoviesAsync(
            ISender sender,
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllMoviesAdminQuery(search, categoryId, isActive, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetMovieByIdAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetMovieDetailAdminQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> CreateMovieAsync(
            ISender sender,
            [FromBody] CreateMovieRequest request)
        {
            var command = new CreateMovieCommand(
                request.Title,
                request.Description,
                request.Image,
                request.BasePrice,
                request.Duration,
                request.CategoryId,
                request.IsActive,
                request.IsHomePage);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { id = result.Value, message = "Tạo phim thành công" })
                : HandlerFailure(result);
        }

        private static async Task<IResult> UpdateMovieAsync(
            ISender sender,
            Guid id,
            [FromBody] UpdateMovieRequest request)
        {
            var command = new UpdateMovieCommand(
                id,
                request.Title,
                request.Description,
                request.Image,
                request.BasePrice,
                request.Duration,
                request.CategoryId,
                request.IsActive,
                request.IsHomePage,
                request.ClearBanner,
                request.ClearPoster);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> DeleteMovieAsync(
            ISender sender,
            Guid id)
        {
            var command = new DeleteMovieCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : HandlerFailure(result);
        }

        private static async Task<IResult> UploadMovieImagesAsync(
            Guid id,
            ISender sender,
            IFormFile? banner,
            IFormFile? poster,
            CancellationToken cancellationToken)
        {
            Stream? bannerStream = null;
            string? bannerFileName = null;
            string? bannerContentType = null;

            if (banner != null)
            {
                bannerStream = banner.OpenReadStream();
                bannerFileName = banner.FileName;
                bannerContentType = banner.ContentType;
            }

            Stream? posterStream = null;
            string? posterFileName = null;
            string? posterContentType = null;

            if (poster != null)
            {
                posterStream = poster.OpenReadStream();
                posterFileName = poster.FileName;
                posterContentType = poster.ContentType;
            }

            if (bannerStream == null && posterStream == null)
            {
                return Results.BadRequest(new { error = "Vui lòng chọn ít nhất một tệp tin banner hoặc poster để tải lên." });
            }

            var command = new UploadMovieImagesCommand(
                id,
                bannerStream,
                bannerFileName,
                bannerContentType,
                posterStream,
                posterFileName,
                posterContentType);

            var result = await sender.Send(command, cancellationToken);

            if (bannerStream != null) await bannerStream.DisposeAsync();
            if (posterStream != null) await posterStream.DisposeAsync();

            return result.IsSuccess
                ? Results.Ok(new { message = "Tải ảnh lên thành công" })
                : HandlerFailure(result);
        }
    }
}
