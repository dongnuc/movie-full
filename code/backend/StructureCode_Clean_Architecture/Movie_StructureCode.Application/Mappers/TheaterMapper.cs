using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Mappers
{
    public static class TheaterMapper
    {
        public static TheaterResponse ToResponse(Theater theater)
        {
            return new TheaterResponse(
                theater.Id,
                theater.Name,
                theater.Location,
                theater.IsActive,
                theater.DateCreate,
                theater.DateUpdate);
        }
    }
}
