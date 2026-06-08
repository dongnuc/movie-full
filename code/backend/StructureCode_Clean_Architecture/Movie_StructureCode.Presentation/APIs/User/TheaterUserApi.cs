using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Theaters.Common;
using Movie_StructureCode.Application.Features.UseCase.User.Theaters.Queries.GetActiveTheatersUser;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class TheaterUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/theaters";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("TheaterUser")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("User - Theaters")
                .AllowAnonymous();

            group.MapGet("/", GetActiveTheatersAsync)
                .WithName("GetActiveTheatersUser")
                .WithSummary("Lấy danh sách rạp chiếu phim đang hoạt động kèm số suất chiếu (không phân trang)")
                .Produces<List<TheaterUserResponse>>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> GetActiveTheatersAsync(
            ISender sender,
            [FromQuery] Guid movieId,
            [FromQuery] DateOnly? date)
        {
            var query = new GetActiveTheatersUserQuery(movieId, date);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
