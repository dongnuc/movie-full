using System.Security.Claims;
using Movie_StructureCode.Application.Abstractions.Services.Auth;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    /// <summary>
    /// Combined token services - bao gồm cả ITokenProvider và IRefreshTokenService
    /// Dùng để DI injection một interface duy nhất
    /// </summary>
    public interface ITokenService : ITokenProvider, IRefreshTokenService
    {
        // Combines both interfaces - methods inherited from both
    }
}
