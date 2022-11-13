using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Entities
{
    public class Role : IdentityRole<long>, IEntity
    {
        public Role()
        {
        }

        public Role(string roleName) : base(roleName)
        {
        }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public static class RoleNames
    {
        public const string Admin = nameof(Admin);

        public const string Memeber = nameof(Memeber);

        public static IEnumerable<string> All => new string[] { Admin, Memeber };
    }
}
