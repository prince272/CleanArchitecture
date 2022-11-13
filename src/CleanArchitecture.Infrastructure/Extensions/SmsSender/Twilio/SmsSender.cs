namespace CleanArchitecture.Infrastructure.Extensions.SmsSender.Twilio
{
    public class SmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string body, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
