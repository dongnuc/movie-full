using Microsoft.EntityFrameworkCore.Storage;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);

        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);

    }
}
