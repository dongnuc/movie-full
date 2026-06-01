using System;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common
{
    public static class RoomErrors
    {
        public static Error NotFound(Guid id) =>
            new("Room.NotFound", $"Room with ID {id} was not found.");

        public static Error NameAlreadyExists =>
            new("Room.NameAlreadyExists", "A room with the same name already exists in this theater.");
    }
}
