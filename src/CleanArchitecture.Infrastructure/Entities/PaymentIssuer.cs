using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Entities
{
    [Owned]
    public class PaymentIssuer
    {
        public string Code { get; set; } = null!;

        public string Pattern { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}
