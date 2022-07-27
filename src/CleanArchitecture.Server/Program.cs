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
using CleanArchitecture.Server.Extensions.Identity;
using CleanArchitecture.Server.Extensions;

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
    .AddAuthenticationManager(options =>
    {
        options.Issuer = builder.Configuration["Authentication:Default:Issuer"];
        options.Audience = builder.Configuration["Authentication:Default:Audience"];
        options.Secret = builder.Configuration["Authentication:Default:Secret"];

        options.AccessTokenTimeSpan = TimeSpan.FromMinutes(1);
        options.RefeshTokenTimeSpan = TimeSpan.FromMinutes(5);

        options.MultipleAuthentication = true;

    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add authentication and authorization services.
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:Default:Issuer"], // site that makes the token.
            ValidateIssuer = true,
            ValidAudience = builder.Configuration["Authentication:Default:Audience"], // site that consumes the token.
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Default:Secret"])),
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
                    context.HttpContext.RequestServices.GetRequiredService<AuthenticationManager>();
                return authenticationManager.ValidateAsync(context);
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
    });

builder.Services.AddAuthorization();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});

builder.Services.AddAntiforgery(options => options.HeaderName = AuthenticationManager.XSRF_TOKEN_KEY);


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
    ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, expression) =>
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

        var propertyName = RelovePropertyName();
        return propertyName != null ? JsonNamingPolicy.CamelCase.ConvertName(propertyName) : null;
    };

    options.DisableDataAnnotationsValidation = true;
    options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
