using System.Collections.Generic;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Categories.Common;
using Movie_StructureCode.Application.Features.UseCase.User.Categories.Queries.GetActiveCategoriesUser;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class CategoryUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/categories";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("CategoryUser")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("User - Categories")
                .AllowAnonymous();

            group.MapGet("/", GetActiveCategoriesAsync)
                .WithName("GetActiveCategoriesUser")
                .WithSummary("Lấy danh sách thể loại phim đang hoạt động (không phân trang)")
                .Produces<List<CategoryUserResponse>>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> GetActiveCategoriesAsync(ISender sender)
        {
            var query = new GetActiveCategoriesUserQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
