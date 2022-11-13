using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Extensions.FileStorage.Local
{
    public static class LocalFileStorageExtensions
    {
        public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, Action<LocalFileStorageOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<IFileStorage, LocalFileStorage>();
            return services;
        }
    }
}
