using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Domain.Abstractions
{
    public interface IAccountQueryService
    {
        Task<PagedResult<AccountResponse>> GetAccountsPagedAsync(
            string? search,
            string? role,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
