using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    [Owned]
    public class Media
    {
        public string Name { get; set; } = null!;

        public int? Width { get; set; }

        public int? Height { get; set; }

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
