using AutoMapper;
using CleanArchitecture.Core.Entities;

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

        public IDictionary<string, string> Roles = new Dictionary<string, string>();
    }

    public class ProfileProfile : Profile
    {
        public ProfileProfile()
        {
            CreateMap<User, ProfileModel>();
        }
    }
}
