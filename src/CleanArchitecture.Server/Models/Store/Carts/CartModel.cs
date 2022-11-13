using AutoMapper;
using CleanArchitecture.Infrastructure.Entities.Store;
using CleanArchitecture.Server.Models.Store.Products;

namespace CleanArchitecture.Server.Models.Store.Carts
{
    public class CartModel
    {
        public long Id { get; set; }

        public ProductModel Product { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<Cart, CartModel>();
        }
    }
}
