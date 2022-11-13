using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Entities.Store
{
    [Owned]
    public class OrderItem
    {
        public string ProductName { get; set; } = null!;

        public long ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public Media? Image { get; set; }
    }
}
