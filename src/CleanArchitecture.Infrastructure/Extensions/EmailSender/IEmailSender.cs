using System.Net.Mail;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.EmailSender
{
    public interface IEmailSender
    {
        Task SendAsync(EmailAccount emailFrom, string emailTo, string subject, string body, CancellationToken cancellationToken = default);
    }
}