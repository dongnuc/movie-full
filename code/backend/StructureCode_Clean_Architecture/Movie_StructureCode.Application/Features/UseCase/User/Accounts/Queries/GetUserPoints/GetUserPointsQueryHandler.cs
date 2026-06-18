using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Accounts.Queries.GetUserPoints
{
    internal sealed class GetUserPointsQueryHandler : IQueryHandler<GetUserPointsQuery, UserPointsResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserPointsQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserPointsResponse>> Handle(GetUserPointsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserPointsResponse>(new Error("User.NotFound", "Không tìm thấy thông tin tài khoản người dùng."));
            }

            return Result.Success(new UserPointsResponse(user.Points));
        }
    }
}
