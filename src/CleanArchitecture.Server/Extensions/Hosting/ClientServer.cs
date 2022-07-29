using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Server.Extensions.Hosting
{
    public class ClientServer : IClientServer
    {
        private readonly IServer _server;
        private readonly IOptions<ClientServerOptions> _peerOptions;

        public ClientServer(IServer server, IOptions<ClientServerOptions> peerOptions)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _peerOptions = peerOptions ?? throw new ArgumentNullException(nameof(peerOptions));
        }

        public string[] ServerUrls
        {
            get
            {
                var serverUrls = (_server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>()).ToArray();
                return serverUrls;
            }
        }

        public string[] ClientUrls
        {
            get
            {
                return _peerOptions.Value.ClientUrls;
            }
        }

        public bool IsClientUrl(string url)
        {
            if (ClientUrls.Any(clientUrl => Uri.Compare(new Uri(clientUrl), new Uri(url), UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.InvariantCultureIgnoreCase) == 0)) 
                return true;

            return false;
        }

        public string PrimaryServerUrl => ServerUrls.First();

        public string PrimaryClientUrl => ClientUrls.First();


    }
}