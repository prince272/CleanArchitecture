using AutoMapper;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Infrastructure.Entities.Store;

namespace CleanArchitecture.Server.Models.Store.Products
{
    public class EditProductForm
    {
        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public bool Published { get; set; }

        public List<Media> Images { get; set; } = new List<Media>();

        public string Description { get; set; } = null!;
    }

    public class EditProductProfile : Profile
    {
        public EditProductProfile()
        {
            CreateMap<EditProductForm, Product>();
        }
    }
}
