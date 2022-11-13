using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Entities
{
    [Owned]
    public class Media
    {
        public string Name { get; set; } = null!;

        public MediaType Type { get; set; }

        public string MimeType { get; set; } = null!;

        public long Size { get; set; }

        public string Path { get; set; } = null!;
    }

    public enum MediaType
    {
        Audio,
        Video,
        Document,
        Image
    }
}
