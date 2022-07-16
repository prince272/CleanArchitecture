using CleanArchitecture.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class UserBearerToken : IEntity
    {
        public long Id { get; set; }

        public string AccessTokenHash { get; set; } = null!;

        public DateTimeOffset AccessTokenExpiresDateTime { get; set; }

        public string RefreshTokenSerialHash { get; set; } = null!;

        public string? RefreshTokenSerialSourceHash { get; set; }

        public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
