using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class Role : IdentityRole<long>, IEntity
    {
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole : IdentityUserRole<long>, IEntity
    {
        public virtual User User { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
    }
}
