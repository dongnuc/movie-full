using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    public interface IAuthenticationService
    {
        Task<Result<LoginResponse>> LoginAsync(
            string usernameOrEmail, 
            string password, 
            CancellationToken cancellationToken = default);
    }
}
