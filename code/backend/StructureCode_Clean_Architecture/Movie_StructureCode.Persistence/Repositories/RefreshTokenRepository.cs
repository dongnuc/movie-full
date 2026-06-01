using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    /// <summary>
    /// Refresh token repository - handles all database operations for refresh tokens
    /// </summary>
    public sealed class RefreshTokenRepository : Repository<RefreshToken>, IRepository<RefreshToken>
    {
        public RefreshTokenRepository(AppDbContext context) : base(context) { }
    }
}
