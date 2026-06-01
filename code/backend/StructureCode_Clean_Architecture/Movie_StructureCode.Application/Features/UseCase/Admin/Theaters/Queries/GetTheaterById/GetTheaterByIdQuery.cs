using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetTheaterById
{
    public sealed record GetTheaterByIdQuery(Guid Id) : IQuery<TheaterResponse>;
}
