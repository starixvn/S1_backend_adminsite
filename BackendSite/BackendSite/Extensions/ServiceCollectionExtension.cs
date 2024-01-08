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


namespace BackendSite.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDatabaseConnection(this IServiceCollection services, IConfiguration config)
        {
            var provider = services.BuildServiceProvider();
            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            var serviceType = config.GetValue<string>("ServiceType");

            var reportDBConn = config.GetConnectionString($"{serviceType}_ReportDB");
            var reportDB34Conn = config.GetConnectionString($"{serviceType}_ReportDB_34");       
            var reportDB32Conn = config.GetConnectionString($"{serviceType}_ReportDB_32");

            //只有PRD環境有設定34跟32的連線字串
            if (!string.IsNullOrEmpty(reportDB34Conn))
            {
                // PRD ReportDB使用36當Master，34當backup
                services.AddSingleton(new MySqlClient(DataBase.Report, logger, reportDBConn, reportDB34Conn));
                // PRD TestReportDB指定使用34當Master，32當backup
                services.AddSingleton(new MySqlClient(DataBase.TestReport, logger, reportDB34Conn, reportDB32Conn));
            }
            else
            {
                services.AddSingleton(new MySqlClient(DataBase.Report, logger, reportDBConn));
            }

            services.AddSingleton(new MySqlClient(DataBase.Main, logger, config.GetConnectionString($"{serviceType}_MainDB")));
            services.AddSingleton(new MySqlClient(DataBase.Wager, logger, config.GetConnectionString($"{serviceType}_WagerDB")));
            services.AddSingleton(new MySqlClient(DataBase.Log, logger, config.GetConnectionString($"{serviceType}_LogDB")));
            services.AddSingleton(new MySqlClient(DataBase.IP, logger, config.GetConnectionString($"{serviceType}_IPTable")));
            return services;
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