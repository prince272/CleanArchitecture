using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class Payment : IEntity
    {
        public long Id { get; set; }

        public string Description { get; set; } = null!;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public string Reference { get; set; } = null!;

        public string? Method { get; set; }
    } 

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Cancelled,
        Declined,
        Expired
    }
}
