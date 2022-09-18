using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Server;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;
using FluentValidation;
using CleanArchitecture.Server.Extensions.Hosting;
using CleanArchitecture.Server.Extensions.Authentication;
using CleanArchitecture.Server.Extensions.Anonymous;
using CleanArchitecture.Infrastructure.Extensions.EmailSender.MailKit;
using CleanArchitecture.Infrastructure.Extensions.FileStorage.Local;
using CleanArchitecture.Infrastructure.Extensions.ViewRenderer.Razor;
using CleanArchitecture.Infrastructure.Extensions.SmsSender.Twilio;
using CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch;
using CleanArchitecture.Infrastructure.Extensions.PaymentProvider;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings objects.
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add database services.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {

    }));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add identity services.
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 0;
    options.Password.RequiredUniqueChars = 0;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters = null;
    options.User.RequireUniqueEmail = false;

    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Generate Short Code for Email Confirmation using Asp.Net Identity core 2.1
    // source: https://stackoverflow.com/questions/53616142/generate-short-code-for-email-confirmation-using-asp-net-identity-core-2-1
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;

    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
    options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
    options.ClaimsIdentity.SecurityStampClaimType = ClaimTypes.SerialNumber;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add authentication and authorization services.
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Default:Issuer"), // site that makes the token.
            ValidateIssuer = true,
            ValidAudience = builder.Configuration.GetValue<string>("Authentication:Default:Audience"), // site that consumes the token.
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Authentication:Default:Secret"))),
            ValidateIssuerSigningKey = true, // verify signature to avoid tampering.
            ValidateLifetime = true, // validate the expiration.
            ClockSkew = TimeSpan.Zero // tolerance for the expiration date.

        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger(nameof(JwtBearerEvents));
                logger.LogError($"Authentication failed {context.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var authenticationManager =
                    context.HttpContext.RequestServices.GetRequiredService<BearerTokenProvider>();
                return authenticationManager.ValidateTokenContextAsync(context);
            },
            OnMessageReceived = context => { return Task.CompletedTask; },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger(nameof(JwtBearerEvents));
                logger.LogError($"OnChallenge error {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    })
    .AddBearerTokenProvider(options =>
    {
        options.Issuer = builder.Configuration.GetValue<string>("Authentication:Default:Issuer");
        options.Audience = builder.Configuration.GetValue<string>("Authentication:Default:Audience");
        options.Secret = builder.Configuration.GetValue<string>("Authentication:Default:Secret");

        // source: https://cloud.google.com/apigee/docs/api-platform/antipatterns/oauth-long-expiration
        options.AccessTokenExpiresIn = TimeSpan.FromMinutes(30);
        options.RefeshTokenExpiresIn = TimeSpan.FromDays(200);

        options.MultipleAuthentication = true;

    })
    .AddGoogle("google", options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = builder.Configuration.GetValue<string>("Authentication:Google:ClientId");
        options.ClientSecret = builder.Configuration.GetValue<string>("Authentication:Google:ClientSecret");
        options.AccessDeniedPath = "/account/access-denied";
    });

builder.Services.AddAnonymous(options => {
    options.HttpOnly = true;
    options.SameSite = SameSiteMode.None;
    options.Expiration = TimeSpan.FromDays(30);
    options.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(defaultPolicy =>
    {
        defaultPolicy
        .WithOrigins(builder.Configuration.GetSection("ClientUrls").Get<string[]>())
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition")
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddAntiforgery(options => options.HeaderName = BearerTokenProvider.XSRF_TOKEN_KEY);

builder.Services.AddMailKitEmailSender(options =>
{
    options.Port = builder.Configuration.GetValue<int>("Mailing:Port");
    options.Hostname = builder.Configuration.GetValue<string>("Mailing:Hostname");
    options.UseServerCertificateValidation = builder.Configuration.GetValue<bool>("Mailing:UseServerCertificateValidation");
    options.SecureSocketId = builder.Configuration.GetValue<int>("Mailing:SecureSocketId");
});

builder.Services.AddSmsSender(options => { });

builder.Services.AddClientServer(options => {
    options.ClientUrls = builder.Configuration.GetSection("ClientUrls").Get<string[]>(); 
});

builder.Services.AddLocalFileStorage(options =>
{
    options.RootPath = builder.Environment.WebRootPath;
});

builder.Services.AddPaySwitchProvider(Options => { });

builder.Services.AddResponseCompression();

builder.Services.AddControllers(options =>
{
    // Form field is required even if not defined so
    // source: https://stackoverflow.com/questions/72060349/form-field-is-required-even-if-not-defined-so
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.Domain = null;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

    options.SlidingExpiration = true;

    options.LoginPath = "/authentication/redirect";
    options.LogoutPath = "/accounts/signout";
    options.ReturnUrlParameter = "returnUrl";


    // Not creating a new object since ASP.NET Identity has created
    // one already and hooked to the OnValidatePrincipal event.
    // See https://github.com/aspnet/AspNetCore/blob/5a64688d8e192cacffda9440e8725c1ed41a30cf/src/Identity/src/Identity/IdentityServiceCollectionExtensions.cs#L56
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});


builder.Services.AddFluentValidation(options =>
{
    ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

    ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) =>
    {
        string? RelovePropertyName()
        {
            if (expression != null)
            {
                var chain = FluentValidation.Internal.PropertyChain.FromExpression(expression);
                if (chain.Count > 0) return chain.ToString();
            }

            if (memberInfo != null)
            {
                return memberInfo.Name;
            }

            return null;
        }

        return RelovePropertyName()?.Humanize();
    };

    options.DisableDataAnnotationsValidation = true;
    options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorViewRenderer((options) => {
    options.RootPathFormat = "/Views/Templates/{0}";
});

var app = builder.Build();

app.UseExceptionHandler("/error/500");
app.UseStatusCodePagesWithReExecute("/error/{0}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAnonymous();
app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCompression();
app.MapControllers();

app.Run();
