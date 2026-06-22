using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Domain.Abstractions
{
    public interface ITheaterMemberQueryService
    {
        Task<PagedResult<TheaterMemberResponse>> GetMembersPagedAsync(
            Guid theaterId,
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
