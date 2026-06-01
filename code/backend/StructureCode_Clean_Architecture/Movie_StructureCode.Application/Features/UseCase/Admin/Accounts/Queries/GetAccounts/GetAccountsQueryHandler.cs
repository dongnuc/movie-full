using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Domain.Abstractions;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Queries.GetAccounts
{
    internal sealed class GetAccountsQueryHandler : IQueryHandler<GetAccountsQuery, PagedResult<AccountResponse>>
    {
        private readonly IAccountQueryService _accountQueryService;

        public GetAccountsQueryHandler(IAccountQueryService accountQueryService)
        {
            _accountQueryService = accountQueryService;
        }

        public async Task<Result<PagedResult<AccountResponse>>> Handle(
            GetAccountsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _accountQueryService.GetAccountsPagedAsync(
                request.Search,
                request.Role,
                request.Page,
                request.PageSize,
                cancellationToken);

            return Result.Success(result);
        }
    }
}
