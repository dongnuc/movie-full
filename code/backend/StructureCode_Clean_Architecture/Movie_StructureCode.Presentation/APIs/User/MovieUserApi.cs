using System;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieDetailUser;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesUser;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class MovieUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/movies";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("MovieUser")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("User - Movies");

            group.MapGet("/", GetMoviesAsync)
                .WithName("GetMoviesUser")
                .WithSummary("Lấy danh sách phim (chỉ phim active)")
                .Produces<PagedResult<MovieUserResponse>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", GetMovieDetailAsync)
                .WithName("GetMovieDetailUser")
                .WithSummary("Lấy chi tiết phim kèm danh sách rạp + suất chiếu hôm nay")
                .Produces<MovieDetailUserResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> GetMoviesAsync(
            ISender sender,
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMoviesUserQuery(search, categoryId, page, pageSize);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetMovieDetailAsync(
            ISender sender,
            Guid id)
        {
            var query = new GetMovieDetailUserQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
