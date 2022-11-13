namespace CleanArchitecture.Infrastructure.Entities
{
    public class Address : IEntity
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Company { get; set; }

        public string ZipCode { get; set; } = null!;

        public string Street { get; set; } = null!;

        public string City { get; set; } = null!;

        public string RegionCode { get; set; } = null!;

        public string RegionName { get; set; } = null!;

        public string CountryCode { get; set; } = null!;

        public string CountryName { get; set; } = null!;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
