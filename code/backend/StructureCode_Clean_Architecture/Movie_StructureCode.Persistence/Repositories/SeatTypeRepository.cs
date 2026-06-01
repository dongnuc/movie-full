using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class SeatTypeRepository : Repository<SeatType>, ISeatTypeRepository
    {
        public SeatTypeRepository(AppDbContext context) : base(context) { }

        public async Task<PagedResult<SeatType>> GetAllAsync(
            Guid? id,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.SeatTypes
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(s => s.Id == id.Value);

            return await PagedResult<SeatType>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<SeatType>> GetActiveSeatTypesAsync(CancellationToken ct = default)
        {
            return await _context.SeatTypes
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }
    }
}
