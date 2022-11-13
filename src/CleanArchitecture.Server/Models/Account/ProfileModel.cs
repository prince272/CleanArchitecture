using AutoMapper;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Server.Extensions.Authentication;

namespace CleanArchitecture.Server.Models.Account
{
    public class ProfileModel
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Name => $"{FirstName} {LastName}";

        public string Email { get; set; } = null!;

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public bool PhoneNumberConfirmed { get; set; }

        public DateTimeOffset RegisteredOn { get; set; }

        public IDictionary<string, string> Roles = new Dictionary<string, string>();
    }

    public class ProfileProfile : Profile
    {
        public ProfileProfile()
        {
            CreateMap<User, ProfileModel>();
        }
    }

    public class BearerTokenModel
    {
        public string TokenType { get; set; } = null!;

        public string AccessToken { get; set; } = null!;

        public long AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; } = null!;

        public long RefreshTokenExpiresIn { get; set; }

        public ProfileModel User { get; set; } = null!;
    }

    public class BearerTokenProfile : Profile
    {
        public BearerTokenProfile()
        {
            CreateMap<BearerTokenData, BearerTokenModel>();
        }
    }
}
