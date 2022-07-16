using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Identity
{
    public class BearerTokenOptions
    {
        public string Secret { set; get; } = null!;

        public string Issuer { set; get; } = null!;

        public string Audience { set; get; } = null!;

        public TimeSpan AccessTokenExpiresTimeSpan { set; get; }

        public TimeSpan RefeshTokenExpiresTimeSpan { set; get; }

        public bool AllowMultipleSignInFromTheSameUser { set; get; }

        public bool AllowSignOutAllUserActiveClients { set; get; }
    }
}
