using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Entities
{
    public class User : IdentityUser<long>, IEntity
    {
        public Guid Guid { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTimeOffset RegisteredAt { get; set; }

        public virtual IEnumerable<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
