namespace CleanArchitecture.Infrastructure.Entities
{
    public class BearerToken : IEntity
    {
        public long Id { get; set; }

        public string AccessTokenHash { get; set; } = null!;

        public string RefreshTokenHash { get; set; } = null!;

        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public DateTimeOffset RefreshTokenExpiresAt { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
