using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Queries.GetAccounts
{
    public sealed record GetAccountsQuery(
        string? Search,
        string? Role,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<AccountResponse>>;
}
