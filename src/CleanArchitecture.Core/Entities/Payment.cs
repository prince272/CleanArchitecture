using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class Payment : IEntity, IExtendable
    {
        public long Id { get; set; }

        public string Description { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentType Type { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }

        public long? UserId { get; set; }

        public DateTimeOffset? CompletedOn { get; set; }

        public DateTimeOffset? ExpiredOn { get; set; }

        public DateTimeOffset? DeclinedOn { get; set; }

        public PaymentMethod? Method { get; set; }

        public string? Gateway { get; set; }

        public string? TransactionId { get; set; }

        public string? CheckoutId { get; set; }

        public string? IPAddress { get; set; }

        public string? UAString { get; set; }

        public string? MobileNumber { get; set; }

        public PaymentIssuer? MobileIssuer { get; set; }

        public string? ReturnUrl { get; set; }

        public string? ExtensionData { get; set; }

        public Payment Renew()
        {
            return new Payment
            {
                Id = 0,
                Description = Description,
                Amount = Amount,
                Type = Type,
                Status = PaymentStatus.Pending,
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = null,
                UserId = UserId,
                CompletedOn = null,
                ExpiredOn = null,
                DeclinedOn = null,
                Method = null,
                Gateway = null,
                TransactionId = null,
                CheckoutId = CheckoutId,
                IPAddress = null,
                UAString = null,
                MobileNumber = null,
                MobileIssuer = null,
                ReturnUrl = null,
                ExtensionData = ExtensionData
            };
        }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Declined,
        Expired
    }

    public enum PaymentType
    {
        Debit,
        Credit
    }

    [Flags]
    public enum PaymentMethod
    {
        Default,
        PlasticMoney,
        MobileMoney
    }
}