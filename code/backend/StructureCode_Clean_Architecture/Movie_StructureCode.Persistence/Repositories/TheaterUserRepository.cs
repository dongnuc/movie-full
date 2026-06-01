using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class TheaterUserRepository : Repository<Theater_User>, ITheaterUserRepository
    {
        public TheaterUserRepository(AppDbContext context) : base(context) { }
    }
}
