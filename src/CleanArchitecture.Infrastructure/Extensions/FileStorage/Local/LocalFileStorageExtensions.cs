using CleanArchitecture.Infrastructure.Extensions.EmailSender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
