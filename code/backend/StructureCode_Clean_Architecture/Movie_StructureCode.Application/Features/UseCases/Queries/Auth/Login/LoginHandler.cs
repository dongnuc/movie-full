using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login
{
    public sealed class LoginHandler : IQueryHandler<Login.Query, LoginResponse>
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<Result<LoginResponse>> Handle(
            Login.Query request,
            CancellationToken cancellationToken)
        {
            return await _authenticationService.LoginAsync(
                request.UsernameOrEmail,
                request.Password,
                cancellationToken);
        }
    }
}
