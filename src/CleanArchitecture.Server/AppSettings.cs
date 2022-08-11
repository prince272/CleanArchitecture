using CleanArchitecture.Infrastructure.Extensions.EmailSender;

namespace CleanArchitecture.Server
{
    public class AppSettings
    {
        public MailingOptions Mailing { get; set; } = null!;
    }

    public class MailingOptions
    {
        public IDictionary<string, EmailAccount> Accounts { get; set; } = new Dictionary<string, EmailAccount>();
    }
}
