using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Movie_StructureCode.Application.Features.UseCase.User.Showings.Common;
using Movie_StructureCode.Application.Features.UseCase.User.Showings.Queries.GetShowingsUser;
using Movie_StructureCode.Presentation.Abstractions;

namespace Movie_StructureCode.Presentation.APIs.User
{
    public class ShowingUserApi : ApiEndpoint, ICarterModule
    {
        private const string BaseUrl = "/api/v{version:apiVersion}/showings";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("ShowingUser")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .WithTags("User - Showings");

            group.MapGet("/", GetShowingsAsync)
                .WithName("GetShowingsUser")
                .WithSummary("Lấy danh sách suất chiếu theo phim, rạp và ngày")
                .Produces<IEnumerable<ShowingUserResponse>>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> GetShowingsAsync(
            ISender sender,
            [FromQuery] Guid movieId,
            [FromQuery] Guid theaterId,
            [FromQuery] string date)
        {
            if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParseExact(date, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" }, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out var dateParsed))
            {
                return Results.BadRequest(new { message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng yyyy-MM-dd hoặc dd/MM/yyyy" });
            }

            var query = new GetShowingsUserQuery(movieId, theaterId, dateParsed);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : HandlerFailure(result);
        }
    }
}
