using CleanArchitecture.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class AuthenticationToken : IEntity
    {
        public long Id { get; set; }

        public string AccessTokenHash { get; set; } = null!;

        public string RefreshTokenHash { get; set; } = null!;

        public DateTimeOffset AccessTokenExpiresOn { get; set; }

        public DateTimeOffset RefreshTokenExpiresOn { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
