using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Server.Extensions.AnonymousId
{
    public class AnonymousCookieOptions : CookieOptions
    {
        public string Name { get; set; }

        public bool SlidingExpiration { get; set; } = true;

        public int Timeout { get; set; }
    }
}