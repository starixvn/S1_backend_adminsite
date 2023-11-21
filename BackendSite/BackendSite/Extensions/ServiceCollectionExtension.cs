using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Infrastructure.Model;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
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
        public static void AddMessageQueueConnection(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            var rabbitMqConfig = configBll.GetConfigValueByEnvironment<RabbitMqConfig>("RabbitMQ");

            var hostName = rabbitMqConfig.HostName;
            var userName = rabbitMqConfig.UserName;
            var password = rabbitMqConfig.Password;
            services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password
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

        public static void DecoratorReferralManagement(this IServiceCollection services)
        {
            services.AddTransient<IReferralManagementBLL>(provider =>
            {
                var referralManagementService = provider.GetRequiredService<ReferralManagementService>();
                var adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
                var reportService = provider.GetRequiredService<ReportService>();
                var logger = provider.GetRequiredService<ILogger<ReferralManagementDecorator>>();

                return new ReferralManagementDecorator(
                    new ReferralManagementBLL(referralManagementService),
                    adjustmentLogReportService, reportService, logger
                );
            });
        }

        public static void DecoratorLiveRegistration(this IServiceCollection services)
        {
            services.AddTransient<ILiveRegistrationBLL>(provider =>
            {
                var liveRegistrationService = provider.GetRequiredService<ILiveRegistrationService>();
                var memberService = provider.GetRequiredService<IMemberService>();
                var serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
                var adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
                var custInfoBll = provider.GetRequiredService<CustInfoBLL>();
                var logger = provider.GetRequiredService<ILogger<LiveRegistrationDecorator>>();

                return new LiveRegistrationDecorator(custInfoBll,
                    new LiveRegistrationBLL(liveRegistrationService, memberService, serverInfoBll),
                    adjustmentLogReportService, logger
                );
            });
        }

        public static IServiceCollection AddSiteConfig(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            // 準備ServerInfo
            var serverInfoList = new List<ServerInfo>() { new ServerInfo() { SiteId = 0, SiteName = "default" } };
            serverInfoList.AddRange(provider.GetRequiredService<IEnumerable<ServerInfo>>());

            // 準備Config
            var siteConfigGroup = provider.GetRequiredService<IEnumerable<Config>>().GroupBy(i => i.SiteId);

            foreach (var serverInfo in serverInfoList)
            {
                //設定SiteConfig
                var siteConfig = siteConfigGroup.FirstOrDefault(i => i.Key == serverInfo.SiteId) ?? siteConfigGroup.First(i => i.Key == 0);
                var config = siteConfig.ToDictionary(i => i.ConfigKey, i => i.ConfigValue);
                services.AddSingleton(new BackendSite.Service.BLL.Infrastructure.Model.SiteConfig(serverInfo, config));
            }
            return services;
        }

        public static IServiceCollection AddCloudFrontImageInfo(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            var siteCloudFrontImageInfo = configBll.GetConfigValuesBySiteName<SiteCloudFrontImageInfo>("SiteCloudFrontImageInfo");
            services.AddSingleton(siteCloudFrontImageInfo);
            return services;
        }

        public static IServiceCollection AddAffConfig(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            var affConfig = configBll.GetConfigValue<AffConfig>("AffConfig");
            services.AddSingleton(affConfig);
            return services;
        }
    }
}