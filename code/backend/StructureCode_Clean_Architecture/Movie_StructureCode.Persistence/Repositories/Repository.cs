using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default)
        {
            return _dbSet.AsNoTracking()
                         .Where(predicate)
                         .ToListAsync(ct)
                         .ContinueWith(t => (IEnumerable<T>)t.Result, ct);
        }

        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _dbSet.FindAsync([id], ct).AsTask();

        public Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default)
            => _dbSet.AnyAsync(predicate, ct);

        public Task AddAsync(T entity, CancellationToken ct = default)
            => _dbSet.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _dbSet.AddRangeAsync(entities, ct);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Remove(T entity)
            => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities)
            => _dbSet.RemoveRange(entities);

        public async Task<bool> ExistsByNameAsync(string value, string? propertyName = null, Guid? excludeId = null, CancellationToken ct = default)
        {
            var type = typeof(T);
            var property = string.IsNullOrEmpty(propertyName)
                ? (type.GetProperty("Name") ?? type.GetProperty("Title"))
                : type.GetProperty(propertyName);

            if (property == null)
            {
                return false;
            }

            var parameter = Expression.Parameter(type, "x");
            var propertyAccess = Expression.Property(parameter, property);
            var constant = Expression.Constant(value);
            Expression expression = Expression.Equal(propertyAccess, constant);

            if (excludeId.HasValue)
            {
                var idProperty = type.GetProperty("Id");
                if (idProperty != null)
                {
                    var idAccess = Expression.Property(parameter, idProperty);
                    var excludeIdConstant = Expression.Constant(excludeId.Value);
                    var idInequality = Expression.NotEqual(idAccess, excludeIdConstant);
                    expression = Expression.AndAlso(expression, idInequality);
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);

            return await _dbSet.AnyAsync(lambda, ct);
        }
    }
}
