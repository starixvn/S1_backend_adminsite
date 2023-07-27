using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using RabbitMQ.Client;

namespace BackendSite.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDatabaseConnection(this IServiceCollection services, IConfiguration config)
        {
            var provider = services.BuildServiceProvider();
            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            var serviceType = config.GetValue<string>("ServiceType");

            return services
                .AddSingleton(param => new MySqlClient(config.GetConnectionString($"{serviceType}_MainDB"), logger)
                {
                    DataBase = DataBase.Main
                })
                .AddSingleton(param => new MySqlClient(config.GetConnectionString($"{serviceType}_ReportDB"), logger)
                {
                    DataBase = DataBase.Report
                })
                .AddSingleton(param => new MySqlClient(config.GetConnectionString($"{serviceType}_WagerDB"), logger)
                {
                    DataBase = DataBase.Wager
                })
                .AddSingleton(param => new MySqlClient(config.GetConnectionString($"{serviceType}_LogDB"), logger)
                {
                    DataBase = DataBase.Log
                }).AddSingleton(param => new MySqlClient(config.GetConnectionString($"{serviceType}_IPTable"), logger)
                {
                    DataBase = DataBase.IP
                });
        }
        public static void AddMessageQueueConnection(this IServiceCollection services, IConfiguration config)
        {
            var hostName = config.GetSection("RabbitMQ:HostName");
            var userName = config.GetSection("RabbitMQ:UserName");
            var password = config.GetSection("RabbitMQ:Password");
            services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
            {
                HostName = hostName.Value,
                UserName = userName.Value,
                Password = password.Value
            });
        }
        public static IServiceCollection AddOptionListByDB<T>(this IServiceCollection services, string sql, object parameters = null)
        {
            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<ConfigService>();
            var option = configService.Query<T>(sql, parameters);
            services.AddSingleton(typeof(IEnumerable<T>), p => option);
            return services;
        }
    }
}