using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Extensions.EmailSender;

namespace CleanArchitecture.Server
{
    public class AppSettings
    {
        public MailingOptions Mailing { get; set; } = null!;

        public MediaOptions Media { get; set; } = null!;
    }

    public class MailingOptions
    {
        public IDictionary<string, EmailAccount> Accounts { get; set; } = new Dictionary<string, EmailAccount>();
    }

    public class MediaOptions
    {
        public IDictionary<MediaType, MediaRule> Rules { get; set; } = new Dictionary<MediaType, MediaRule>();

        public class MediaRule
        {
            public MediaRule()
            {

            }

            public string[] FileTypes { get; set; } = null!;

            public long FileSize { get; set; }

            public MediaType MediaType { get; set; }
        }
    }
}
