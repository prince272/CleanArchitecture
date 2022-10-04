#nullable disable

using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Server.Extensions.Anonymous
{
    public class AnonymousCookieOptions : CookieOptions
    {
        public string Name { get; set; }

        public bool SlidingExpiration { get; set; } = true;

        public int Timeout { get; set; }
    }
}