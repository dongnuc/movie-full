using Microsoft.EntityFrameworkCore.Storage;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context) => _context = context;

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            await transaction.CommitAsync(ct);
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            await transaction.RollbackAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
