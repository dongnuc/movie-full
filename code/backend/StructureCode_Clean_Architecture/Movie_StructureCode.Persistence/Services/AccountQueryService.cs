using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Abstractions;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Services
{
    public class AccountQueryService : IAccountQueryService
    {
        private readonly AppDbContext _context;

        public AccountQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AccountResponse>> GetAccountsPagedAsync(
            string? search,
            string? role,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            // Start the query joining Users, UserRoles, and Roles
            var query = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId into userRolesJoin
                        from ur in userRolesJoin.DefaultIfEmpty()
                        join r in _context.Roles on ur.RoleId equals r.Id into rolesJoin
                        from r in rolesJoin.DefaultIfEmpty()
                        select new
                        {
                            User = u,
                            RoleName = r != null ? r.Name : "User"
                        };

            // Apply search filter (Username or Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(x => 
                    (x.User.UserName != null && x.User.UserName.ToLower().Contains(cleanSearch)) || 
                    (x.User.Email != null && x.User.Email.ToLower().Contains(cleanSearch)));
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(x => x.RoleName == role);
            }

            // Get total count
            int totalCount = await query.CountAsync(cancellationToken);

            // Fetch paginated items, including active TheaterUsers mappings
            var dbItems = await query
                .OrderBy(x => x.User.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.User.Id,
                    Username = x.User.UserName ?? string.Empty,
                    Email = x.User.Email ?? string.Empty,
                    Role = x.RoleName ?? "User",
                    // Active if LockoutEnd is null or in the past
                    IsActive = x.User.LockoutEnd == null || x.User.LockoutEnd <= DateTimeOffset.UtcNow,
                    TheaterIds = _context.TheaterUsers
                        .Where(tu => tu.UserId == x.User.Id && tu.IsActive)
                        .Select(tu => tu.TheaterId)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var items = dbItems.Select(x => new AccountResponse(
                x.Id,
                x.Username,
                x.Email,
                x.Role,
                x.IsActive,
                x.TheaterIds
            )).ToList();

            return PagedResult<AccountResponse>.Create(items, page, pageSize, totalCount);
        }
    }
}
