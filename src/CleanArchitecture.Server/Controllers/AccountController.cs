using AutoMapper;
using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Extensions.EmailSender;
using CleanArchitecture.Infrastructure.Extensions.SmsSender;
using CleanArchitecture.Infrastructure.Extensions.ViewRenderer;
using CleanArchitecture.Server.Extensions.Authentication;
using CleanArchitecture.Server.Extensions.Hosting;
using CleanArchitecture.Server.Models;
using CleanArchitecture.Server.Models.Account;
using CleanArchitecture.Server.Utilities;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Mail;
using System.Security.Claims;

namespace CleanArchitecture.Server.Controllers
{
    public class AccountController : ApiController
    {
        private readonly BearerTokenProvider _bearerTokenProvider;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IViewRenderer _viewRenderer;
        private readonly IClientServer _clientServer;
        private readonly IMapper _mapper;

        public AccountController(
            BearerTokenProvider bearerTokenProvider,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IOptions<AppSettings> appSettings,
            IViewRenderer viewRenderer,
            IClientServer clientServer,
            IMapper mapper)
        {
            _bearerTokenProvider = bearerTokenProvider ?? throw new ArgumentNullException(nameof(bearerTokenProvider));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _clientServer = clientServer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("account/register")]
        public async Task<IActionResult> Register([FromBody] CreateAccountForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<CreateAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user != null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' is already registered.");
                return ValidationProblem(errors);
            }

            user = new User();
            var userContactType = ContactHelper.Switch(form.Username);

            switch (userContactType)
            {
                case ContactType.EmailAddress: user.Email = form.Username; break;
                case ContactType.PhoneNumber: user.PhoneNumber = form.Username; break;
                default: throw new InvalidOperationException();
            }
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.UserName = await SecurityHelper.GenerateSlugAsync($"{form.FirstName} {form.LastName}".ToLowerInvariant(),
                "_", userName => _userManager.Users.AnyAsync(_ => _.UserName == userName));
            user.RegisteredOn = DateTimeOffset.UtcNow;

            (await _userManager.CreateAsync(user, form.Password)).ThrowIfFailed();

            foreach (var roleName in RoleNames.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    (await _roleManager.CreateAsync(new Role(roleName))).ThrowIfFailed();
            }

            if (await _userManager.Users.LongCountAsync() == 1)
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Admin, RoleNames.Memeber })).ThrowIfFailed();
            else
                (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Memeber })).ThrowIfFailed();

            return Ok();
        }

        [HttpPost("account/verify")]
        public async Task<IActionResult> SendVerifyAccount([FromBody] SendVerifyAccountForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<SendVerifyAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);
            if (user == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' does not exist.");
                return ValidationProblem(errors);
            }

            var formUsernameType = ContactHelper.Switch(form.Username);

            switch (formUsernameType)
            {
                case ContactType.EmailAddress:
                    {
                        ((IVerifyAccountForm)form).Code = await _userManager.GenerateChangeEmailTokenAsync(user, form.Username);

                        var message = new
                        {
                            From = _appSettings.Value.Mailing.Accounts["Support"],
                            To = form.Username,
                            Subject = $"Confirm Your {formUsernameType.Humanize(LetterCasing.Title)}",
                            Body = await _viewRenderer.RenderToStringAsync("Email/VerifyAccount", (user, (IVerifyAccountForm)form, formUsernameType))
                        };

                        await _emailSender.SendAsync(message.From, message.To, message.Subject, message.Body);
                    }
                    break;
                case ContactType.PhoneNumber:
                    {
                        ((IVerifyAccountForm)form).Code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, form.Username);

                        var message = new
                        {
                            PhoneNumber = form.Username,
                            Body = HtmlHelper.StripHtml(await _viewRenderer.RenderToStringAsync("Sms/VerifyAccount", (user, (IVerifyAccountForm)form, formUsernameType)))
                        };

                        await _smsSender.SendAsync(message.PhoneNumber, message.Body);
                    }
                    break;

                default: throw new InvalidOperationException();
            }

            return Ok();
        }

        [HttpPut("account/verify")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<VerifyAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);
            if (user == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' does not exist.");
                return ValidationProblem(errors);
            }

            var result = default(IdentityResult);

            var userContactType = ContactHelper.Switch(form.Username);

            switch (userContactType)
            {
                case ContactType.EmailAddress: result = await _userManager.ChangeEmailAsync(user, user.Email, form.Code); break;
                case ContactType.PhoneNumber: result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, form.Code); break;
                default: throw new InvalidOperationException();
            }

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>();

                if (result.Errors.Any(_ => _.Code == "InvalidToken"))
                    errors.Add(() => form.Code, $"'{nameof(form.Code).Humanize()}' is not valid.");

                return ValidationProblem(errors, detail: result.Errors.Select(_ => _.Description).Humanize());
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("account/change")]
        public async Task<IActionResult> SendChangeAccount([FromBody] SendVerifyAccountForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<SendVerifyAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user != null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' is already registered.");
                return ValidationProblem(errors);
            }

            var formUsernameType = ContactHelper.Switch(form.Username);

            switch (formUsernameType)
            {
                case ContactType.EmailAddress:
                    {
                        ((IVerifyAccountForm)form).Code = await _userManager.GenerateChangeEmailTokenAsync(currentUser, form.Username);

                        var message = new
                        {
                            From = _appSettings.Value.Mailing.Accounts["Support"],
                            To = form.Username,
                            Subject = $"Change Your {formUsernameType.Humanize(LetterCasing.Title)}",
                            Body = await _viewRenderer.RenderToStringAsync("Email/ChangeAccount", (currentUser, (IVerifyAccountForm)form, formUsernameType))
                        };

                        await _emailSender.SendAsync(message.From, message.To, message.Subject, message.Body);
                    }
                    break;
                case ContactType.PhoneNumber:
                    {
                        ((IVerifyAccountForm)form).Code = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, form.Username);

                        var message = new
                        {
                            PhoneNumber = form.Username,
                            Body = HtmlHelper.StripHtml(await _viewRenderer.RenderToStringAsync("Sms/ChangeAccount", (currentUser, (IVerifyAccountForm)form, formUsernameType)))
                        };

                        await _smsSender.SendAsync(message.PhoneNumber, message.Body);
                    }
                    break;

                default: throw new InvalidOperationException();
            }

            return Ok();
        }

        private async Task<ProfileModel> GetProfileModelAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var profile = _mapper.Map<ProfileModel>(user);
            profile.Roles = (await _userManager.GetRolesAsync(user)).ToDictionary(key => key, value => value.Humanize());
            return profile;
        }

        [Authorize]
        [HttpGet("account/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");
            return Ok(await GetProfileModelAsync(currentUser));
        }

        [Authorize]
        [HttpPut("account/change")]
        public async Task<IActionResult> ChangeAccount([FromBody] VerifyAccountForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<VerifyAccountValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user != null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' is already registered.");
                return ValidationProblem(errors);
            }

            var formUsernameType = ContactHelper.Switch(form.Username);
            IdentityResult result;

            switch (formUsernameType)
            {
                case ContactType.EmailAddress: result = await _userManager.ChangeEmailAsync(currentUser, currentUser.Email, form.Code); break;
                case ContactType.PhoneNumber: result = await _userManager.ChangePhoneNumberAsync(currentUser, currentUser.PhoneNumber, form.Code); break;
                default: throw new InvalidOperationException();
            }

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>();

                if (result.Errors.Any(_ => _.Code == "InvalidToken"))
                    errors.Add(() => form.Code, $"'{nameof(form.Code).Humanize()}' is not valid.");

                return ValidationProblem(errors, detail: result.Errors.Select(_ => _.Description).Humanize());
            }

            return Ok();
        }

        [HttpPost("account/password/reset")]
        public async Task<IActionResult> SendResetPassword([FromBody] SendResetPasswordForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<SendResetPasswordValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);
            if (user == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' does not exist.");
                return ValidationProblem(errors);
            }

            var userContactType = ContactHelper.Switch(form.Username);

            switch (userContactType)
            {
                case ContactType.EmailAddress:
                    {
                        ((IResetPasswordForm)form).Code = await _userManager.GeneratePasswordResetTokenAsync(user);

                        var message = new
                        {
                            From = _appSettings.Value.Mailing.Accounts["Support"],
                            To = form.Username,
                            Subject = $"Reset Your Password",
                            Body = await _viewRenderer.RenderToStringAsync("Email/ResetPassword", (user, form))
                        };

                        await _emailSender.SendAsync(message.From, message.To, message.Subject, message.Body);
                    }
                    break;
                case ContactType.PhoneNumber:
                    {
                        ((IResetPasswordForm)form).Code = await _userManager.GeneratePasswordResetTokenAsync(user);

                        var message = new
                        {
                            PhoneNumber = form.Username,
                            Body = HtmlHelper.StripHtml(await _viewRenderer.RenderToStringAsync("Sms/ResetPassword", (user, form)))
                        };

                        await _smsSender.SendAsync(message.PhoneNumber, message.Body);
                    }
                    break;

                default: throw new InvalidOperationException();
            }

            return Ok();
        }

        [HttpPut("account/password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<ResetPasswordValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);
            if (user == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' does not exist.");
                return ValidationProblem(errors);
            }

            var result = await _userManager.ResetPasswordAsync(user, form.Code, form.Password);

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>();

                if (result.Errors.Any(_ => _.Code == "InvalidToken"))
                    errors.Add(() => form.Code, $"'{nameof(form.Code).Humanize()}' is not valid.");

                return ValidationProblem(errors, detail: result.Errors.Select(_ => _.Description).Humanize());
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("account/password/change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<ChangePasswordValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

            var result = await _userManager.ChangePasswordAsync(currentUser, form.CurrentPassword, form.NewPassword);

            if (!result.Succeeded)
            {
                var errors = new Dictionary<string, string[]>();

                if (result.Errors.Any(_ => _.Code == "PasswordMismatch"))
                    errors.Add(() => form.CurrentPassword, $"'{nameof(form.CurrentPassword).Humanize()}' is not correct.");

                return ValidationProblem(errors, detail: result.Errors.Select(_ => _.Description).Humanize());
            }

            return Ok();
        }

        [HttpPost("account/token/generate")]
        public async Task<IActionResult> Generate([FromBody] CreateAccountTokenForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<CreateAccountTokenValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var user = await _userManager.FindByUsernameAsync(form.Username);

            if (user == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' is not registered.");
                return ValidationProblem(errors);
            }

            if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Username).Humanize()}' is not confirmed.");
                return ValidationProblem(errors);
            }

            if (!await _userManager.CheckPasswordAsync(user, form.Password))
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.Username, $"'{ContactHelper.Switch(form.Password).Humanize()}' is not correct.");
                return ValidationProblem(errors);
            }

            var data = _mapper.Map<BearerTokenModel>(await _bearerTokenProvider.GenerateTokenAsync(user));
            data.User = await GetProfileModelAsync(user);
            return Ok(data);
        }

        [HttpPost("account/{provider}/token/generate")]
        public async Task<IActionResult> Generate([FromRoute] string provider)
        {
            if (provider == null) return ValidationProblem(title: $"'{nameof(provider)}' is required.");

            var signinInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (signinInfo == null)
                return ValidationProblem(title: "No external sign-in information provided.");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(signinInfo.LoginProvider, signinInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            var email = signinInfo.Principal.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    await _userManager.RemoveLoginAsync(user, signinInfo.LoginProvider, signinInfo.ProviderKey);
                    var result = await _userManager.AddLoginAsync(user, signinInfo);

                    if (result.Succeeded)
                    {
                        var data = _mapper.Map<BearerTokenModel>(await _bearerTokenProvider.GenerateTokenAsync(user));
                        data.User = await GetProfileModelAsync(user);
                        return Ok(data);
                    }
                }
                else
                {
                    var firstName = signinInfo.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
                    var lastName = signinInfo.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty; ;

                    user = new User();
                    user.Email = email;
                    user.EmailConfirmed = true;
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.UserName = await SecurityHelper.GenerateSlugAsync($"{firstName} {lastName}".ToLowerInvariant(),
                                 "_", userName => _userManager.Users.AnyAsync(_ => _.UserName == userName));
                    user.RegisteredOn = DateTimeOffset.UtcNow;

                    (await _userManager.CreateAsync(user, Guid.NewGuid().ToString())).ThrowIfFailed();

                    foreach (var roleName in RoleNames.All)
                    {
                        if (!await _roleManager.RoleExistsAsync(roleName))
                            (await _roleManager.CreateAsync(new Role(roleName))).ThrowIfFailed();
                    }

                    if (await _userManager.Users.LongCountAsync() == 1)
                        (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Admin, RoleNames.Memeber })).ThrowIfFailed();
                    else
                        (await _userManager.AddToRolesAsync(user, new string[] { RoleNames.Memeber })).ThrowIfFailed();

                    var result = await _userManager.AddLoginAsync(user, signinInfo);

                    if (result.Succeeded)
                    {
                        var data = _mapper.Map<BearerTokenModel>(await _bearerTokenProvider.GenerateTokenAsync(user));
                        data.User = await GetProfileModelAsync(user);
                        return Ok(data);
                    }
                }
            }

            return ValidationProblem(title: "External sign-in information is not valid.");
        }

        [HttpPost("account/token/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshAccountTokenForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<RefreshAccountTokenValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());


            var token = await _bearerTokenProvider.FindTokenAsync(form.RefreshToken);
            if (token == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.RefreshToken, $"'{ContactHelper.Switch(form.RefreshToken).Humanize()}' is not valid.");
                return ValidationProblem(errors);
            }

            var user = token.User;
            var data = _mapper.Map<BearerTokenModel>(await _bearerTokenProvider.RenewTokenAsync(token));
            data.User = await GetProfileModelAsync(user);
            return Ok(data);
        }

        [HttpPost("account/token/revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshAccountTokenForm form)
        {
            var formState = await HttpContext.RequestServices.GetRequiredService<RefreshAccountTokenValidator>().ValidateAsync(form);
            if (formState.Errors.Any()) return ValidationProblem(formState.ToDictionary());

            var token = await _bearerTokenProvider.FindTokenAsync(form.RefreshToken);
            if (token == null)
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add(() => form.RefreshToken, $"'{ContactHelper.Switch(form.RefreshToken).Humanize()}' is not valid.");
                return ValidationProblem(errors);
            }

            await _bearerTokenProvider.RevokeTokenAsync(token);
            return Ok();
        }

        [HttpGet("account/{provider}/connect")]
        public IActionResult ExternalSignIn([FromRoute] string provider, string returnUrl)
        {
            if (provider == null) return ValidationProblem(title: $"'{nameof(provider)}' is required.");
            if (returnUrl == null) return ValidationProblem(title: $"'{nameof(returnUrl)}' is required.");

            if (!_clientServer.IsClientUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Request a redirect to the external sign-in provider.
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
            return Challenge(properties, provider);
        }
    }
}