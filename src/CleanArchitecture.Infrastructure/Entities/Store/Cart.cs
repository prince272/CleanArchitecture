namespace CleanArchitecture.Infrastructure.Entities.Store
{
    public class Cart : IEntity
    {
        public long Id { get; set; }

        public virtual Product Product { get; set; } = null!;

        public long ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
