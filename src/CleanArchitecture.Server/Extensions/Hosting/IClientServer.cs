namespace CleanArchitecture.Server.Extensions.Hosting
{
    public interface IClientServer
    {
        string[] ServerUrls { get;  }

        string[] ClientUrls { get; }

        bool IsClientUrl(string url);
    }
}
