using AutoMapper;
using CleanArchitecture.Core;
using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Entities.Store;
using CleanArchitecture.Infrastructure.Services.Store;
using CleanArchitecture.Server.Models.Store.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Server.Controllers.Store
{
    public class ProductsController : ApiController
    {
        private readonly AppDbContext _appDbContext;
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly IMapper _mapper;

        public ProductsController(AppDbContext appDbContext, ProductService productService, CartService cartService, IMapper mapper)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _mapper = mapper;
        }

        [HttpPost("products")]
        public async Task<IActionResult> Create(CreateProductForm form)
        {
            var product = _mapper.Map(form, new Product());

            product.CreatedAt = DateTimeOffset.Now;
            product.PublishedAt = form.Published ? product.PublishedAt ?? DateTimeOffset.UtcNow : null;

            await _productService.UpdateSlugAsync(product);
            await _productService.CreateAsync(product);
            return Ok();
        }

        [HttpPut("products/{productId}")]
        public async Task<IActionResult> Update(string productId, CreateProductForm form)
        {
            var product = await _productService.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            product = _mapper.Map(form, product);
            product.UpdatedAt = DateTimeOffset.Now;
            product.PublishedAt = form.Published ? product.PublishedAt ?? DateTimeOffset.UtcNow : null;

            await _productService.UpdateSlugAsync(product);
            await _productService.UpdateAsync(product);
            return Ok();
        }

        [HttpDelete("products/{productId}")]
        public async Task<IActionResult> Delete(string productId)
        {
            var product = await _productService.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            await _productService.DeleteAync(product);
            return Ok();
        }

        [HttpGet("products/{productId}")]
        public async Task<IActionResult> Get(string productId)
        {
            var product = await _productService.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            var model = _mapper.Map(product, new ProductModel());
            return Ok(model);
        }

        [HttpGet("products")]
        public async Task<IActionResult> Get(string[]? productIds, int pageNumber, int pageSize, decimal? minPrice, decimal? maxPrice)
        {
            var query = _appDbContext.Set<Product>().AsQueryable();
            var model = new PageModel(await query.LongCountAsync(), pageNumber, pageSize);
            model.Items = await _mapper.ProjectTo<ProductModel>(((model.SkippedItems > 0 ? query.Skip(model.SkippedItems) : query).Take(model.PageSize)))
                .ToArrayAsync();
            return Ok(model);
        }

        [HttpPost("products/{productId}/cart")]
        public async Task<IActionResult> Cart(string productId, int quantity)
        {
            var product = await _productService.GetProductAsync(productId);

            if (product == null)
                return NotFound();

            var cart = await _cartService.GetCartByProductAsync(product.Id);

            if (quantity == 0)
            {
                if (cart != null)
                {
                    await _cartService.DeleteAync(cart);
                }
            }
            else
            {
                if (cart == null)
                {
                    cart = new Cart();
                    cart.CreatedAt = DateTimeOffset.UtcNow;
                    cart.ProductId = product.Id;
                    cart.Quantity = quantity;
                    await _cartService.CreateAsync(cart);
                }
                else
                {
                    cart.UpdatedAt = DateTimeOffset.UtcNow;
                    cart.Quantity = quantity;
                    await _cartService.UpdateAsync(cart);
                }
            }

            return Ok();
        }
    }
}