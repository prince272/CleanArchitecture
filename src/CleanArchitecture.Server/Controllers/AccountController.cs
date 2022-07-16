using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Server.Models.Account;
using CleanArchitecture.Server.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BearerTokenManager _bearerTokenManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AccountController(BearerTokenManager bearerTokenManager, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateAccountModel form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<CreateAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(ModelState.Include(formState.Errors));

            var user = new User();

            switch (ContactHelper.Switch(form.Username))
            {
                case ContactType.EmailAddress: user.Email = form.Username; break;
                case ContactType.PhoneNumber: user.PhoneNumber = form.Username; break;
                default: throw new InvalidOperationException();
            }

            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.UserName = await SecurityHelper.GenerateSlugAsync($"{form.FirstName} {form.LastName}".ToLowerInvariant(),
                "_", slug => _userManager.Users.AnyAsync(_ => _.UserName == slug));
            user.Created = DateTimeOffset.UtcNow;

            (await _userManager.CreateAsync(user, form.Password)).ThrowIfFailed();

            foreach (var roleName in RoleNames.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new Role(roleName));
            }

            if (await _userManager.Users.LongCountAsync() == 1)
            {
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Admin, RoleNames.Memeber })).ThrowIfFailed();
            }
            else
            {
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Memeber })).ThrowIfFailed();
            }

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateModel form)
        {
            return Ok();
        }
    }
}
