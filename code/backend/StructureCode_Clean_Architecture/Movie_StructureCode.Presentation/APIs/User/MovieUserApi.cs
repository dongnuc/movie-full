using System;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieDetailUser;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesUser;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesFeaturedToday;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesWithoutShowings;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieHomePageUser;
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
                .WithTags("User - Movies")
                .AllowAnonymous();

            group.MapGet("/", GetMoviesAsync)
                .WithName("GetMoviesUser")
                .WithSummary("Lấy danh sách phim (chỉ phim active)")
                .Produces<PagedResult<MovieUserResponse>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", GetMovieDetailAsync)
                .WithName("GetMovieDetailUser")
                .WithSummary("Lấy chi tiết phim kèm danh sách rạp + suất chiếu hôm nay")
                .Produces<MovieDetailUserResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/today-featured", GetMoviesFeaturedTodayAsync)
                .WithName("GetMoviesFeaturedToday")
                .WithSummary("Lấy danh sách tối đa 12 phim có nhiều suất chiếu nhất từ hôm nay đến 7 ngày tiếp theo")
                .Produces<List<MovieUserResponse>>(StatusCodes.Status200OK);

            group.MapGet("/upcoming", GetMoviesWithoutShowingsAsync)
                .WithName("GetMoviesWithoutShowings")
                .WithSummary("Lấy danh sách tối đa 10 phim active chưa có suất chiếu")
                .Produces<List<MovieUserResponse>>(StatusCodes.Status200OK);

            group.MapGet("/homepage", GetMovieHomePageAsync)
                .WithName("GetMovieHomePageUser")
                .WithSummary("Lấy phim Trang chủ (Active = true và IsHomePage = true)")
                .Produces<MovieHomePageResponse>(StatusCodes.Status200OK)
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

        private static async Task<IResult> GetMoviesFeaturedTodayAsync(
            ISender sender)
        {
            var query = new GetMoviesFeaturedTodayQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetMoviesWithoutShowingsAsync(
            ISender sender)
        {
            var query = new GetMoviesWithoutShowingsQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }

        private static async Task<IResult> GetMovieHomePageAsync(
            ISender sender)
        {
            var query = new GetMovieHomePageUserQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
