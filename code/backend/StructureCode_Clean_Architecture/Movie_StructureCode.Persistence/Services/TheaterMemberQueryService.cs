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
    public class TheaterMemberQueryService : ITheaterMemberQueryService
    {
        private readonly AppDbContext _context;

        public TheaterMemberQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TheaterMemberResponse>> GetMembersPagedAsync(
            Guid theaterId,
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = from tu in _context.TheaterUsers
                        join u in _context.Users on tu.UserId equals u.Id
                        where tu.TheaterId == theaterId && tu.IsActive
                        select u;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(cleanSearch)) ||
                    (u.Email != null && u.Email.ToLower().Contains(cleanSearch)));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var dbUsers = await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = new List<TheaterMemberResponse>();
            foreach (var u in dbUsers)
            {
                var roleNames = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where ur.UserId == u.Id
                                       select r.Name)
                                      .ToListAsync(cancellationToken);

                var isActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow;

                items.Add(new TheaterMemberResponse(
                    u.Id,
                    u.UserName ?? string.Empty,
                    u.Email ?? string.Empty,
                    roleNames.Where(r => r != null).Cast<string>().ToList(),
                    isActive
                ));
            }

            return PagedResult<TheaterMemberResponse>.Create(items, page, pageSize, totalCount);
        }
    }
}
