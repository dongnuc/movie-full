using System;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common
{
    public static class SeatTypeErrors
    {
        public static Error NotFound(Guid id) => 
            new("SeatType.NotFound", $"SeatType with ID {id} was not found.");

        public static readonly Error NameAlreadyExists = 
            new("SeatType.NameAlreadyExists", "A seat type with the same name already exists.");
    }
}
