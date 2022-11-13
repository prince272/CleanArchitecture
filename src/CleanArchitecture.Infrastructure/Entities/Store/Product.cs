namespace CleanArchitecture.Infrastructure.Entities.Store
{
    public class Product : IEntity
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public List<Media> Images { get; set; } = new List<Media>();

        public string Description { get; set; } = null!;

        public string Summary { get; set; } = null!;
    }
}