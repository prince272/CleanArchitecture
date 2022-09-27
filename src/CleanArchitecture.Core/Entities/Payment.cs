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

        public decimal Amount { get; set; }

        public PaymentType Type { get; set; }

        public PaymentStatus Status { get; set; }

        public string TransactionId { get; set; } = null!;
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

    public enum PaymentType
    {
        Debit,
        Credit
    }
}
