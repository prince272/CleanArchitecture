using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions.Authentication
{
    public class AuthenticationTokenProvider
    {
        public const string XSRF_TOKEN_KEY = "XSRF-TOKEN";
        public string TokenType => "bearer";

        private readonly IOptionsSnapshot<AuthenticationTokenOptions> _authenticationOptions;
        private readonly ILogger<AuthenticationTokenProvider> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAntiforgery _antiforgery;
        private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions;

        public AuthenticationTokenProvider(
            IOptionsSnapshot<AuthenticationTokenOptions> authenticationOptions,
            ILogger<AuthenticationTokenProvider> logger,
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IHttpContextAccessor contextAccessor,
            IAntiforgery antiforgery,
            IOptions<AntiforgeryOptions> antiforgeryOptions)
        {
            _authenticationOptions = authenticationOptions ?? throw new ArgumentNullException(nameof(authenticationOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            _antiforgeryOptions = antiforgeryOptions ?? throw new ArgumentNullException(nameof(antiforgery));
        }

        public async Task<AuthenticationTokenObject> GenerateTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await RemoveExpiredTokensByUserIdAsync(user.Id);

            if (!_authenticationOptions.Value.MultipleAuthentication)
                await RemoveTokensByUserIdAsync(user.Id);

            var now = DateTimeOffset.UtcNow;
            var (accessToken, claims) = await GenerateAccessTokenAsync(now, user);
            var refreshToken = GenerateRefreshToken(now);

            _unitOfWork.Add(new AuthenticationToken
            {
                UserId = user.Id,

                AccessTokenHash = SecurityHelper.GenerateHash(accessToken),
                RefreshTokenHash = SecurityHelper.GenerateHash(refreshToken),

                AccessTokenExpiresOn = now.Add(_authenticationOptions.Value.AccessTokenExpiresIn),
                RefreshTokenExpiresOn = now.Add(_authenticationOptions.Value.RefeshTokenExpiresIn)
            });
            await _unitOfWork.CompleteAsync();

            RegenerateAntiForgeryCookies(claims);

            return new AuthenticationTokenObject
            {
                TokenType = TokenType,

                AccessToken = accessToken,
                AccessTokenExpiresIn = _authenticationOptions.Value.AccessTokenExpiresIn.TotalSeconds,

                RefreshToken = refreshToken,
                RefreshTokenExpiresIn = _authenticationOptions.Value.RefeshTokenExpiresIn.TotalSeconds,
            };
        }

        public async Task<AuthenticationTokenObject> RenewTokenAsync(AuthenticationToken bearerToken)
        {
            if (bearerToken == null)
                throw new ArgumentNullException(nameof(bearerToken));

            await RemoveExpiredTokensByUserIdAsync(bearerToken.UserId);

            if (!_authenticationOptions.Value.MultipleAuthentication)
                await RemoveTokensByUserIdAsync(bearerToken.UserId);

            await RemoveTokensWithSameRefreshTokenAsync(bearerToken.RefreshTokenHash);

            var now = DateTimeOffset.UtcNow;
            var (accessToken, claims) = await GenerateAccessTokenAsync(now, bearerToken.User);
            var refreshToken = GenerateRefreshToken(now);
            _unitOfWork.Add(new AuthenticationToken
            {
                UserId = bearerToken.UserId,

                AccessTokenHash = SecurityHelper.GenerateHash(accessToken),
                RefreshTokenHash = SecurityHelper.GenerateHash(refreshToken),

                AccessTokenExpiresOn = now.Add(_authenticationOptions.Value.AccessTokenExpiresIn),
                RefreshTokenExpiresOn = now.Add(_authenticationOptions.Value.RefeshTokenExpiresIn)
            });
            await _unitOfWork.CompleteAsync();

            RegenerateAntiForgeryCookies(claims);

            return new AuthenticationTokenObject
            {
                TokenType = TokenType,

                AccessToken = accessToken,
                AccessTokenExpiresIn = _authenticationOptions.Value.AccessTokenExpiresIn.TotalSeconds,

                RefreshToken = refreshToken,
                RefreshTokenExpiresIn = _authenticationOptions.Value.RefeshTokenExpiresIn.TotalSeconds,
            };
        }

        public async Task RevokeTokenAsync(AuthenticationToken bearerToken)
        {
            if (bearerToken == null)
                throw new ArgumentNullException(nameof(bearerToken));

            await RemoveExpiredTokensByUserIdAsync(bearerToken.UserId);

            if (!_authenticationOptions.Value.MultipleAuthentication)
                await RemoveTokensByUserIdAsync(bearerToken.UserId);

            await RemoveTokensWithSameRefreshTokenAsync(bearerToken.RefreshTokenHash);
            await _unitOfWork.CompleteAsync();

            DeleteAntiForgeryCookies();
        }

        public Task<AuthenticationToken?> FindTokenAsync(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            var refreshTokenHash = SecurityHelper.GenerateHash(refreshToken);
            return _unitOfWork.Query<AuthenticationToken>().Include(bt => bt.User).FirstOrDefaultAsync(bt => bt.RefreshTokenHash == refreshTokenHash);
        }

        private async Task<(string AccessToken, IEnumerable<Claim> Claims)> GenerateAccessTokenAsync(DateTimeOffset now, User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleNames = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, SecurityHelper.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iss, _authenticationOptions.Value.Issuer, ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _authenticationOptions.Value.Issuer),

                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(ClaimTypes.Name, user.UserName, ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(ClaimTypes.SerialNumber, user.SecurityStamp, ClaimValueTypes.String, _authenticationOptions.Value.Issuer),
            };

            foreach (var roleName in roleNames)
                claims.Add(new Claim(ClaimTypes.Role, roleName, ClaimValueTypes.String, _authenticationOptions.Value.Issuer));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _authenticationOptions.Value.Issuer,
                _authenticationOptions.Value.Audience,
                claims,
                now.DateTime,
                now.DateTime.Add(_authenticationOptions.Value.AccessTokenExpiresIn),
                creds);
            return (new JwtSecurityTokenHandler().WriteToken(token), claims);
        }

        private string GenerateRefreshToken(DateTimeOffset now)
        {
            var refreshTokenSerial = SecurityHelper.CreateCryptographicallySecureGuid().ToString()
                .Replace("-", "", StringComparison.Ordinal);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, SecurityHelper.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iss, _authenticationOptions.Value.Issuer, ClaimValueTypes.String, _authenticationOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _authenticationOptions.Value.Issuer),

                new(ClaimTypes.SerialNumber, refreshTokenSerial, ClaimValueTypes.String, _authenticationOptions.Value.Issuer)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _authenticationOptions.Value.Issuer,
                _authenticationOptions.Value.Audience,
                claims,
                now.DateTime,
                now.DateTime.Add(_authenticationOptions.Value.RefeshTokenExpiresIn),
                creds);

            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
            return refreshToken;
        }

        private void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims)
        {
            if (_contextAccessor.HttpContext == null)
                throw new InvalidOperationException($"'{ExpressionHelper.GetName(() => _contextAccessor.HttpContext)}' cannot be null.");

            _contextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
            var tokens = _antiforgery.GetAndStoreTokens(_contextAccessor.HttpContext);
            if (tokens.RequestToken == null)
                throw new InvalidOperationException($"'{ExpressionHelper.GetName(() => tokens.RequestToken)}' cannot be null.");

            _contextAccessor.HttpContext.Response.Cookies.Append(XSRF_TOKEN_KEY, tokens.RequestToken,
                new CookieOptions
                {
                    HttpOnly = false // Now JavaScript is able to read the cookie
                });
        }

        private void DeleteAntiForgeryCookies()
        {
            var cookies = _contextAccessor.HttpContext?.Response.Cookies;
            if (cookies is null)
            {
                return;
            }

            var cookieName = _antiforgeryOptions.Value.Cookie.Name;
            if (string.IsNullOrWhiteSpace(cookieName))
            {
                return;
            }

            cookies.Delete(cookieName);
            cookies.Delete(XSRF_TOKEN_KEY);
        }

        private async Task RemoveTokensByUserIdAsync(long userId)
        {
            var bearerTokens = await _unitOfWork.Query<AuthenticationToken>().Where(bt => bt.UserId == userId).ToArrayAsync();
            _unitOfWork.Remove(bearerTokens);
        }

        private async Task RemoveTokensWithSameRefreshTokenAsync(string refreshTokenHash)
        {
            if (refreshTokenHash == null)
                throw new ArgumentNullException(nameof(refreshTokenHash));

            var bearerTokens = await _unitOfWork.Query<AuthenticationToken>().Where(bt => bt.RefreshTokenHash == refreshTokenHash).ToArrayAsync();
            _unitOfWork.Remove(bearerTokens);
        }

        private async Task RemoveExpiredTokensByUserIdAsync(long userId)
        {
            var now = DateTimeOffset.UtcNow;
            var bearerTokens = await _unitOfWork.Query<AuthenticationToken>().Where(bt => bt.UserId == userId && bt.RefreshTokenExpiresOn < now).ToArrayAsync();
            _unitOfWork.Remove(bearerTokens);
        }

        public async Task<bool> ValidateTokenAsync(string accessToken, long userId)
        {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));

            var accessTokenHash = SecurityHelper.GenerateHash(accessToken);
            var bearerToken = await _unitOfWork.Query<AuthenticationToken>().FirstOrDefaultAsync(
                bt => bt.AccessTokenHash == accessTokenHash && bt.UserId == userId);
            return bearerToken?.AccessTokenExpiresOn >= DateTimeOffset.UtcNow;
        }

        public async Task ValidateTokenContextAsync(TokenValidatedContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                context.Fail("This is not our issued token. It has no claims.");
                return;
            }

            var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
            if (serialNumberClaim == null)
            {
                context.Fail("This is not our issued token. It has no serial.");
                return;
            }

            var userIdString = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
            {
                context.Fail("This is not our issued token. It has no user-id.");
                return;
            }

            var user = await _userManager.FindByIdAsync(userIdString);
            if (user == null || !string.Equals(user.SecurityStamp, serialNumberClaim.Value, StringComparison.Ordinal))
            {
                // user has changed his/her password/roles/stat/IsActive
                context.Fail("This token is expired. Please login again.");
                return;
            }

            if (!(context.SecurityToken is JwtSecurityToken accessToken) ||
                string.IsNullOrWhiteSpace(accessToken.RawData) ||
                !await ValidateTokenAsync(accessToken.RawData, userId))
            {
                context.Fail("This token is not in our database.");
                return;
            }
        }
    }
}