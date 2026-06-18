using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetSessionTtl
{
    internal sealed class GetSessionTtlQueryHandler : IQueryHandler<GetSessionTtlQuery, GetSessionTtlResponse>
    {
        private readonly ISeatLockService _seatLockService;

        public GetSessionTtlQueryHandler(ISeatLockService seatLockService)
        {
            _seatLockService = seatLockService;
        }

        public async Task<Result<GetSessionTtlResponse>> Handle(GetSessionTtlQuery request, CancellationToken cancellationToken)
        {
            var remainingSeconds = await _seatLockService.GetSessionRemainingSeconds(request.ShowingId, request.UserId.ToString());
            if (remainingSeconds == null)
            {
                return Result.Success(new GetSessionTtlResponse(request.ShowingId, 0, false));
            }

            return Result.Success(new GetSessionTtlResponse(request.ShowingId, remainingSeconds.Value, true));
        }
    }
}
