using AutoMapper;
using CleanArchitecture.Infrastructure.Entities.Store;

namespace CleanArchitecture.Server.Models.Store.Products
{
    public class ProductModel
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Updated => UpdatedAt != null;

        public DateTimeOffset? PublishedAt { get; set; }

        public bool Published => PublishedAt != null;

        public List<MediaModel> Images { get; set; } = new List<MediaModel>();

        public string Description { get; set; } = null!;

        public string Summary { get; set; } = null!;
    }

    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>();
        }
    }
}
