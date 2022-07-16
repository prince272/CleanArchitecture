using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
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

namespace CleanArchitecture.Infrastructure.Identity
{
    public class BearerTokenManager
    {
        private readonly IOptionsSnapshot<BearerTokenOptions> _bearerTokenOptions;
        private readonly ILogger<BearerTokenManager> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public BearerTokenManager(
            IOptionsSnapshot<BearerTokenOptions> bearerTokenOptions,
            ILogger<BearerTokenManager> logger,
            IUnitOfWork unitOfWork,
            UserManager<User> userManager)
        {
            _bearerTokenOptions = bearerTokenOptions ?? throw new ArgumentNullException(nameof(bearerTokenOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<BearerToken> GenerateBearerTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var now = DateTimeOffset.UtcNow;

            var (accessTokenValue, claims) = await GenerateAccessTokenAsync(now, user);
            var (refreshTokenValue, refreshTokenSerial) = GenerateRefreshToken(now);
            await AddBearerTokenAsync(now, user, refreshTokenSerial, accessTokenValue, null);

            await _unitOfWork.CompleteAsync();

            return new BearerToken
            {
                AccessToken = accessTokenValue,
                RefreshToken = refreshTokenValue,
                Claims = claims
            };
        }

        public async Task<BearerToken> RenewBearerTokenAsync(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            var bearerToken = await FindBearerTokenAsync(refreshToken);
            if (bearerToken == null) throw new InvalidOperationException("Bearer token could not be found.");

            var now = DateTimeOffset.UtcNow;
            var (accessTokenValue, claims) = await GenerateAccessTokenAsync(now, bearerToken.User);
            var (refreshTokenValue, refreshTokenSerial) = GenerateRefreshToken(now);
            await AddBearerTokenAsync(now, bearerToken.User, refreshTokenSerial, accessTokenValue, GetRefreshTokenSerial(refreshTokenValue));

            await _unitOfWork.CompleteAsync();

            return new BearerToken
            {
                AccessToken = accessTokenValue,
                RefreshToken = refreshTokenValue,
                Claims = claims
            };
        }

        public async Task DeleteBearerTokenAsync(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            var bearerToken = await FindBearerTokenAsync(refreshToken);
            if (bearerToken != null)
            {
                _unitOfWork.Remove(bearerToken);
                await _unitOfWork.CompleteAsync();
            }
        }

        public Task<UserBearerToken?> FindBearerTokenAsync(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            var refreshTokenSerial = GetRefreshTokenSerial(refreshToken);
            if (refreshTokenSerial == null)
            {
                return Task.FromResult<UserBearerToken?>(null);
            }

            var refreshTokenSerialHash = SecurityHelper.GenerateHash(refreshTokenSerial);
            return _unitOfWork.Query<UserBearerToken>().Include(bt => bt.User).FirstOrDefaultAsync(bt => bt.RefreshTokenSerialHash == refreshTokenSerialHash);
        }

        public async Task RevokeBearerTokensAsync(long userId, string? refreshToken = null)
        {
            if (_bearerTokenOptions.Value.AllowSignOutAllUserActiveClients)
            {
                await InvalidateBearerTokensAsync(userId);
            }

            if (refreshToken != null)
            {
                var refreshTokenSerial = GetRefreshTokenSerial(refreshToken);

                if (refreshTokenSerial != null)
                {
                    var refreshTokenIdHashSource = SecurityHelper.GenerateHash(refreshTokenSerial);
                    await RemoveBearerTokensWithSameRefreshTokenSourceAsync(refreshTokenIdHashSource);
                }
            }

            await RemoveExpiredBearerTokensAsync();
            await _unitOfWork.CompleteAsync();
        }

        private async Task<(string AccessToken, IEnumerable<Claim> Claims)> GenerateAccessTokenAsync(DateTimeOffset now, User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleNames = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, SecurityHelper.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iss, _bearerTokenOptions.Value.Issuer, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _bearerTokenOptions.Value.Issuer),

                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(ClaimTypes.Name, user.UserName, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(ClaimTypes.SerialNumber, user.SecurityStamp, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),
            };

            foreach (var roleName in roleNames)
                claims.Add(new Claim(ClaimTypes.Role, roleName, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenOptions.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _bearerTokenOptions.Value.Issuer,
                _bearerTokenOptions.Value.Audience,
                claims,
                now.DateTime,
                now.DateTime.Add(_bearerTokenOptions.Value.AccessTokenExpiresTimeSpan),
                creds);
            return (new JwtSecurityTokenHandler().WriteToken(token), claims);
        }

        private (string RefreshToken, string RefreshTokenSerial) GenerateRefreshToken(DateTimeOffset now)
        {
            var refreshTokenSerial = SecurityHelper.CreateCryptographicallySecureGuid().ToString()
                .Replace("-", "", StringComparison.Ordinal);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, SecurityHelper.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iss, _bearerTokenOptions.Value.Issuer, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer),

                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _bearerTokenOptions.Value.Issuer),

                new(ClaimTypes.SerialNumber, refreshTokenSerial, ClaimValueTypes.String, _bearerTokenOptions.Value.Issuer)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenOptions.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _bearerTokenOptions.Value.Issuer,
                _bearerTokenOptions.Value.Audience,
                claims,
                now.DateTime,
                now.DateTime.Add(_bearerTokenOptions.Value.RefeshTokenExpiresTimeSpan),
                creds);

            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
            return (refreshToken, refreshTokenSerial);
        }

        private string? GetRefreshTokenSerial(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            try
            {
                var decodedRefreshTokenPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                    refreshToken,
                    new TokenValidationParameters
                    {
                        ValidIssuer = _bearerTokenOptions.Value.Issuer, // site that makes the token
                        ValidAudience = _bearerTokenOptions.Value.Audience, // site that consumes the token
                        RequireExpirationTime = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenOptions.Value.Secret)),
                        ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                        ValidateLifetime = true, // validate the expiration
                        ClockSkew = TimeSpan.Zero // tolerance for the expiration date
                    },
                    out _
                );

                return decodedRefreshTokenPrincipal.Claims
                    ?.FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.SerialNumber, StringComparison.Ordinal))?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate refreshTokenValue: `{refreshToken}`.");

                return null;
            }
        }

        private async Task AddBearerTokenAsync(DateTimeOffset now, User user, string refreshTokenSerial, string accessToken, string? refreshTokenSerialSource)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (refreshTokenSerial == null)
                throw new ArgumentNullException(nameof(refreshTokenSerial));

            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));

            var bearerToken = new UserBearerToken
            {
                UserId = user.Id,
                // Refresh token handles should be treated as secrets and should be stored hashed
                RefreshTokenSerialHash = SecurityHelper.GenerateHash(refreshTokenSerial),
                RefreshTokenSerialSourceHash = refreshTokenSerialSource != null ? SecurityHelper.GenerateHash(refreshTokenSerialSource) : null,
                AccessTokenHash = SecurityHelper.GenerateHash(accessToken),
                RefreshTokenExpiresDateTime = now.Add(_bearerTokenOptions.Value.RefeshTokenExpiresTimeSpan),
                AccessTokenExpiresDateTime = now.Add(_bearerTokenOptions.Value.AccessTokenExpiresTimeSpan)
            };

            if (!_bearerTokenOptions.Value.AllowMultipleSignInFromTheSameUser)
            {
                await InvalidateBearerTokensAsync(bearerToken.UserId);
            }

            if (bearerToken.RefreshTokenSerialSourceHash != null)
            {
                await RemoveBearerTokensWithSameRefreshTokenSourceAsync(bearerToken.RefreshTokenSerialSourceHash);
            }

            _unitOfWork.Add(bearerToken);
        }

        private async Task InvalidateBearerTokensAsync(long userId)
        {
            var bearerTokens = await _unitOfWork.Query<UserBearerToken>().Where(bt => bt.UserId == userId).ToArrayAsync();
            _unitOfWork.Remove(bearerTokens);
        }

        private async Task RemoveBearerTokensWithSameRefreshTokenSourceAsync(string refreshTokenSerialSourceHash)
        {
            if (refreshTokenSerialSourceHash == null)
                throw new ArgumentNullException(nameof(refreshTokenSerialSourceHash));

            var bearerTokens = await _unitOfWork.Query<UserBearerToken>().Where(bt => bt.RefreshTokenSerialSourceHash == refreshTokenSerialSourceHash ||
                                     bt.RefreshTokenSerialHash == refreshTokenSerialSourceHash &&
                                     bt.RefreshTokenSerialSourceHash == null).ToArrayAsync();

            _unitOfWork.Remove(bearerTokens);
        }

        private async Task RemoveExpiredBearerTokensAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var bearerTokens = await _unitOfWork.Query<UserBearerToken>().Where(bt => bt.RefreshTokenExpiresDateTime < now).ToArrayAsync();
            _unitOfWork.Remove(bearerTokens);
        }

        public async Task<bool> ValidateBearerTokenAsync(string accessToken, long userId)
        {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));

            var accessTokenHash = SecurityHelper.GenerateHash(accessToken);
            var bearerToken = await _unitOfWork.Query<UserBearerToken>().FirstOrDefaultAsync(
                bt => bt.AccessTokenHash == accessTokenHash && bt.UserId == userId);
            return bearerToken?.AccessTokenExpiresDateTime >= DateTimeOffset.UtcNow;
        }

        public async Task ValidateAsync(TokenValidatedContext context)
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

            var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
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
                !await ValidateBearerTokenAsync(accessToken.RawData, userId))
            {
                context.Fail("This token is not in our database.");
                return;
            }
        }
    }
}