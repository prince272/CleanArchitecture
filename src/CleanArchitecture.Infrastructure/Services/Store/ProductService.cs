using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Entities.Store;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Services.Store
{
    public class ProductService
    {
        private readonly AppDbContext _appDbContext;

        public ProductService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        }

        public async Task CreateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Description = Sanitizer.SanitizeHtml(product.Description);
            product.Summary = Sanitizer.StripHtml(product.Description).Truncate(256, Truncator.FixedLength);

            _appDbContext.Add(product);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Description = Sanitizer.SanitizeHtml(product.Description);
            product.Summary = Sanitizer.StripHtml(product.Description).Truncate(256, Truncator.FixedLength);

            _appDbContext.Update(product);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _appDbContext.Remove(product);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<Product?> GetProductAsync(string productId)
        {
            var product = await _appDbContext.Set<Product>()
               .FirstOrDefaultAsync(p => p.Id.ToString() == productId || p.Slug == productId);

            return product;
        }

        public async Task UpdateSlugAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            Func<string, Task<bool>> slugExists = (slug) => _appDbContext.Set<Product>().AnyAsync(p => !p.Slug.Equals(slug));
            product.Slug = (await Algorithm.GenerateSlugAsync(product.Name, slugExists)).ToLowerInvariant();
        }
    }
}