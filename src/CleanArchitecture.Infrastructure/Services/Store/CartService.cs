using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Entities.Store;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Services.Store
{
    public class CartService
    {
        private readonly AppDbContext _appDbContext;

        public CartService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        }

        public async Task CreateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            _appDbContext.Add(cart);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            _appDbContext.Update(cart);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            _appDbContext.Remove(cart);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<Cart?> GetCartByProductAsync(long productId)
        {
            var cart = await _appDbContext.Set<Cart>().FirstOrDefaultAsync(c => c.ProductId == productId);
            return cart;
        }
    }
}
