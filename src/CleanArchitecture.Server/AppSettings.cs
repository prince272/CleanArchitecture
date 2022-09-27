using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Extensions.EmailSender;

namespace CleanArchitecture.Server
{
    public class AppSettings
    {
        public MailSettings MailSettings { get; set; } = null!;

        public MediaSettings MediaSettings { get; set; } = null!;
    }

    public class MailSettings
    {
        public IDictionary<string, EmailAccount> Accounts { get; set; } = new Dictionary<string, EmailAccount>();
    }

    public class MediaSettings
    {
        public IDictionary<MediaType, MediaRule> Rules { get; set; } = new Dictionary<MediaType, MediaRule>();

        public class MediaRule
        {
            public MediaRule()
            {
            }

            public string[] FileExtensions { get; set; } = null!;

            public long FileSize { get; set; }

            public MediaType MediaType { get; set; }
        }
    }
}
