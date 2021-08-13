using BackendSite.Service.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDatabaseConnection(this IServiceCollection services, IConfiguration config)
        {
            var provider = services.BuildServiceProvider();

            var conn = config.GetConnectionString("ConnectionString");

            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            services.AddSingleton(serviceProvider => new MySqlClient(conn, logger)
            {
                Name = "MainDB"
            });

            return services;
        }
    }
}