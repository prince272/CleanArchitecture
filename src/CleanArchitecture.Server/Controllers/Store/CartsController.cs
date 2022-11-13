using AutoMapper;
using CleanArchitecture.Core;
using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Entities.Store;
using CleanArchitecture.Infrastructure.Services.Store;
using CleanArchitecture.Server.Models.Store.Carts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Server.Controllers.Store
{
    public class CartsController : ApiController
    {
        private readonly AppDbContext _appDbContext;
        private readonly CartService _cartService;
        private readonly IMapper _mapper;


        public CartsController(AppDbContext appDbContext, CartService cartService, IMapper mapper)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _mapper = mapper;
        }

        [HttpGet("carts")]
        public async Task<IActionResult> Get(int pageNumber, int pageSize)
        {
            var query = _appDbContext.Set<Cart>().AsQueryable().Include(_ => _.Product);
            var model = new PageModel(await query.LongCountAsync(), pageNumber, pageSize);
            model.Items = await _mapper.ProjectTo<CartModel>(((model.SkippedItems > 0 ? query.Skip(model.SkippedItems) : query).Take(model.PageSize)))
                .ToArrayAsync();
            return Ok(model);
        }
    }
}
