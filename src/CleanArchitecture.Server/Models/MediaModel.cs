using AutoMapper;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Infrastructure.Extensions.FileStorage;

namespace CleanArchitecture.Server.Models
{
    public class MediaModel
    {
        public string Name { get; set; } = null!;

        public MediaType Type { get; set; }

        public string MimeType { get; set; } = null!;

        public long Size { get; set; }

        public string Path { get; set; } = null!;

        public string Url { get; set; } = null!;
    }


    public class MediaProfile : Profile
    {
        public MediaProfile(IFileStorage fileStorage)
        {
            CreateMap<Media, MediaModel>().ForMember(m => m.Url, opt => opt.MapFrom(e => fileStorage.GetUrl(e.Path)));
        }
    }
}
