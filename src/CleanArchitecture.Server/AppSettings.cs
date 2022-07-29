using CleanArchitecture.Infrastructure.Extensions.EmailSender;

namespace CleanArchitecture.Server
{
    public class AppSettings
    {
        public MailingSettings Mailing { get; set; } = null!;
    }

    public class MailingSettings
    {
        public IDictionary<string, EmailAccount> Accounts { get; set; } = new Dictionary<string, EmailAccount>();
    }
}
