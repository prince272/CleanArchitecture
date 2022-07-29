namespace CleanArchitecture.Server.Extensions.Hosting
{
    public static class ClientServerExtensions
    {
        public static IServiceCollection AddClientServer(this IServiceCollection services, Action<ClientServerOptions> configure)
        {
            services.Configure(configure);
            services.AddSingleton<IClientServer, ClientServer>();
            return services;
        }
    }
}
