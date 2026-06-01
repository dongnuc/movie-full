using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken ct = default);
        Task<PagedResult<Category>> GetAllAsync(Guid? id, int pageNumber, int pageSize, CancellationToken ct = default);
    }
}