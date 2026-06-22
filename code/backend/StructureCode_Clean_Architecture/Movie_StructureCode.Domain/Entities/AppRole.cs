using System;
using Microsoft.AspNetCore.Identity;

namespace Movie_StructureCode.Domain.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        public int PermissionMask { get; set; }

        public AppRole() : base()
        {
        }

        public AppRole(string roleName) : base(roleName)
        {
        }
    }
}
