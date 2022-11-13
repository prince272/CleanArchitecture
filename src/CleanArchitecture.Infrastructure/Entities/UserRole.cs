using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Entities
{
    public class UserRole : IdentityUserRole<long>, IEntity
    {
        public virtual User User { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
    }
}
