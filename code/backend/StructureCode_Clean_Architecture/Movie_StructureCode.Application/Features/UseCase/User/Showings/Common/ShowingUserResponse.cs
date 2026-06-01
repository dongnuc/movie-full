using System;

namespace Movie_StructureCode.Application.Features.UseCase.User.Showings.Common
{
    public sealed record ShowingUserResponse(
        Guid Id,
        DateTime TimeStart,
        decimal Price,
        Guid RoomId
    );
}
