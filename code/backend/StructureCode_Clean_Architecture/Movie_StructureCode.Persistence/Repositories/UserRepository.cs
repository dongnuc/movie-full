using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public class UserRepository : Repository<AppUser>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
