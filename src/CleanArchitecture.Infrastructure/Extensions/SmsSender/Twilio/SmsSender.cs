using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
