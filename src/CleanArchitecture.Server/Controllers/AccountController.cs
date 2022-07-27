using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Server.Extensions.Identity;
using CleanArchitecture.Server.Models.Account;
using CleanArchitecture.Server.Utilities;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CleanArchitecture.Server.Controllers
{
    public class AccountController : ApiController
    {
        private readonly AuthenticationManager _authenticationManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AccountController(AuthenticationManager authenticationManager, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _authenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        [HttpPost("account/[action]")]
        public async Task<IActionResult> Create([FromBody] CreateAccountModel form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<CreateAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user != null)
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(form.Username), new[] { $"'{ContactHelper.Switch(form.Username).Humanize()}' is already registered." } }
                });
            }

            user = new User();

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
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Admin, RoleNames.Memeber })).ThrowIfFailed();
            else
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Memeber })).ThrowIfFailed();

            return Ok();
        }

        [HttpPost("account/token/generate")]
        public async Task<IActionResult> GenerateToken([FromBody] CreateAccountTokenModel form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<CreateAccountTokenValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user == null)
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(form.Username), new[] { $"'{ContactHelper.Switch(form.Username).Humanize()}' is not registered." } }
                });
            }

            if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(form.Username), new[] { $"'{ContactHelper.Switch(form.Username).Humanize()}' is not confirmed." } }
                });
            }

            if (!await _userManager.CheckPasswordAsync(user, form.Password))
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(form.Password), new[] { $"'{ContactHelper.Switch(form.Password).Humanize()}' is not correct." } }
                });
            }

            var (accessToken, refreshToken) = await _authenticationManager.GenerateBearerTokenAsync(user);
            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost("account/token/refresh")]
        public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
        {
            if (refreshToken == null) return ValidationProblem(title: $"'{nameof(refreshToken)}' is required.");

            var bearerToken = await _authenticationManager.FindBearerTokenAsync(refreshToken);
            if (bearerToken == null)
                return ValidationProblem(title: $"Invalid '{nameof(refreshToken)}'.");

            var (accessToken, newRefreshToken) = await _authenticationManager.RenewBearerTokenAsync(bearerToken);
            return Ok(new { accessToken, refreshToken = newRefreshToken });
        }

        [HttpPost("account/token/revoke")]
        public async Task<IActionResult> RevokeToken([FromQuery] string refreshToken)
        {
            if (refreshToken == null) return ValidationProblem(title: $"'{nameof(refreshToken)}' is required.");

            var bearerToken = await _authenticationManager.FindBearerTokenAsync(refreshToken);
            if (bearerToken == null)
                return ValidationProblem(title: $"Invalid '{nameof(refreshToken)}'.");

            await _authenticationManager.RevokeBearerTokenAsync(bearerToken);
            return Ok();
        }
    }
}