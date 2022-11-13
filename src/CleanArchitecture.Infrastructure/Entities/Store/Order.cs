using System.Text.Json.Serialization;

namespace CleanArchitecture.Infrastructure.Entities.Store
{
    public class Order : IEntity
    {
        public virtual User User { get; set; } = null!;

        public long UserId { get; set; }

        public long Id { get; set; }

        public string Code { get; set; } = null!;

        public string TrackingCode { get; set; } = null!;

        public string? CancelReason { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }

        public OrderStatus Status { get; set; }

        [JsonPropertyName(nameof(BillingAddress))]
        public Address BillingAddress { get; set; } = null!;

        [JsonPropertyName(nameof(DeliveryAddress))]
        public Address DeliveryAddress { get; set; } = null!;

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Delivering,
        Completed,
        Cancelled
    }
}
