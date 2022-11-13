using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CleanArchitecture.Infrastructure.Extensions.EmailSender.MailKit
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly MailKitEmailSenderOptions _emailSenderOptions;

        public MailKitEmailSender(IOptions<MailKitEmailSenderOptions> emailSenderOptions)
        {
            _emailSenderOptions = emailSenderOptions.Value ?? throw new ArgumentNullException(nameof(emailSenderOptions));
        }

        public async Task SendAsync(EmailAccount emailFrom, string emailTo, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (emailFrom == null) throw new ArgumentNullException(nameof(emailFrom));
            if (emailTo == null) throw new ArgumentNullException(nameof(emailTo));
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var message = new MimeMessage();

            message.Subject = subject;
            message.From.Add(new MailboxAddress(emailFrom.DisplayName, emailFrom.Username));
            message.To.Add(new MailboxAddress(string.Empty, emailTo));

            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            message.Body = builder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => _emailSenderOptions.UseServerCertificateValidation;
                await smtpClient.ConnectAsync(_emailSenderOptions.Hostname, _emailSenderOptions.Port, (SecureSocketOptions)_emailSenderOptions.SecureSocketId, cancellationToken);
                await smtpClient.AuthenticateAsync(emailFrom.Username, emailFrom.Password, cancellationToken);
                await smtpClient.SendAsync(message, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
