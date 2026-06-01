using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class ShowingSeatRepository : Repository<ShowingSeat>, IShowingSeatRepository
    {
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(10);

        public ShowingSeatRepository(AppDbContext context) : base(context) { }

    }
}
