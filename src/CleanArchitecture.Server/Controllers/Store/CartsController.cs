using AutoMapper;
using CleanArchitecture.Core;
using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Entities.Store;
using CleanArchitecture.Infrastructure.Services.Store;
using CleanArchitecture.Server.Models;
using CleanArchitecture.Server.Models.Store.Carts;
using CleanArchitecture.Server.Models.Store.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CleanArchitecture.Server.Controllers.Store
{
    public class CartsController : ApiController
    {
        private readonly AppDbContext _appDbContext;
        private readonly AppSettings _appSettings;
        private readonly CartService _cartService;
        private readonly IMapper _mapper;


        public CartsController(AppDbContext appDbContext, IOptions<AppSettings> appSettingsOptions, CartService cartService, IMapper mapper)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _appSettings = appSettingsOptions.Value ?? throw new ArgumentNullException(nameof(_appSettings));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _mapper = mapper;
        }

        [HttpGet("carts")]
        public async Task<IActionResult> Get(int page, int pageSize)
        {
            var query = _appDbContext.Set<Cart>().AsQueryable();

            page = Math.Min(Math.Max(1, page), _appSettings.SharedSettings.PageLimit);
            pageSize = Math.Min(Math.Max(1, pageSize), _appSettings.SharedSettings.PageLimit);

            var model = new PageModel<ProductModel>(
                items: await _mapper.ProjectTo<ProductModel>(query.Skip((page - 1) * pageSize).Take(pageSize)).ToArrayAsync(),
                totalItems: await query.LongCountAsync(),
                page, pageSize);

            return Ok(model);
        }
    }
}
