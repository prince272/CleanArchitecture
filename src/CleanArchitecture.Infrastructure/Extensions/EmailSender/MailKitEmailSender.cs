using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using MailAddress = System.Net.Mail.MailAddress;
using System.Threading.Tasks;
using System.Text;

namespace CleanArchitecture.Infrastructure.Extensions.EmailSender
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly MailKitEmailSenderOptions emailSenderOptions;

        public MailKitEmailSender(IServiceProvider serviceProvider)
        {
            emailSenderOptions = serviceProvider.GetRequiredService<IOptions<MailKitEmailSenderOptions>>().Value;
        }

        public async Task SendAsync(EmailAccount emailFrom, string emailTo, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();

            message.Subject = subject;
            message.From.Add(new MailboxAddress(emailFrom.DisplayName, emailFrom.Username));
            message.To.Add(new MailboxAddress(string.Empty, emailTo));

            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            message.Body = builder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => emailSenderOptions.UseServerCertificateValidation;
                await smtpClient.ConnectAsync(emailSenderOptions.Hostname, emailSenderOptions.Port, (SecureSocketOptions)emailSenderOptions.SecureSocketId, cancellationToken);
                await smtpClient.AuthenticateAsync(emailFrom.Username, emailFrom.Password, cancellationToken);
                await smtpClient.SendAsync(message, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
