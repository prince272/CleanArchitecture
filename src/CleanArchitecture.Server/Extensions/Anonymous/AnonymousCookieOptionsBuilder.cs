#nullable disable


namespace CleanArchitecture.Server.Extensions.Anonymous
{
    public class AnonymousCookieOptionsBuilder : CookieBuilder
    {
        private const string DEFAULT_COOKIE_NAME = ".ASPXANONYMOUS";
        private const string DEFAULT_COOKIE_PATH = "/";

        public new virtual AnonymousCookieOptions Build(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new AnonymousCookieOptions
            {
                Name = Name ?? DEFAULT_COOKIE_NAME,
                Path = Path ?? DEFAULT_COOKIE_PATH,
                SameSite = SameSite,
                HttpOnly = HttpOnly,
                MaxAge = MaxAge,
                Domain = Domain,
                IsEssential = IsEssential,
                Secure = SecurePolicy == CookieSecurePolicy.Always || SecurePolicy == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps,
                Expires = DateTime.Now.Add(Expiration ?? TimeSpan.FromDays(14))
            };
        }
    }
}