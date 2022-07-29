using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.SmsSender
{
    public interface ISmsSender
    {
        Task SendAsync(string phoneNumber, string body, CancellationToken cancellationToken = default);
    }
}
