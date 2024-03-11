using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Infrastructure.Model;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.SqlClient;
using Elastic.Apm.StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using StackExchange.Redis;

namespace BackendSite
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            var redisConnection = ConnectionMultiplexer.Connect(_configuration.GetValue<string>("RedisConnection"));
            redisConnection.UseElasticApm();
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            services.AddSingleton<ICacheHelper, RedisCacheHelper>();

            /*Library*/
            services.AddSingleton<Tools>();
            services.AddSingleton<Common>();
            services.AddSingleton<CourierAdapter>();
            services.AddSingleton<CodeGenerator>();
            services.AddSingleton<CacheHelper>();
            services.AddDatabaseConnection(_configuration);
            
            
            services.AddSingleton<AWSS3Helper>();
            services.AddSingleton<AWSCloudFrontHelper>();
            services.AddSingleton<Cryptor>();
            
            services.AddSingleton<DateTimeHelper>();
            
            /*DAL*/
            var dalAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in dalAssemblies)
            {
                var list = item.DefinedTypes.Where(t => t.IsClass
                                                        && t.Namespace == "BackendSite.Service.DAL"
                                                        && t.Name.Contains("Service")).ToList();

                list.ForEach((type) =>
                {
                    services.AddSingleton(type, type);
                });

            }

            /*BLL*/
            var bllAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in bllAssemblies)
            {
                var list = item.DefinedTypes.Where(t => t.IsClass
                                                        && t.Namespace == "BackendSite.Service.BLL"
                                                        && t.Name.Contains("BLL")).ToList();

                list.ForEach((type) =>
                {
                    services.AddSingleton(type, type);
                });

            }
            services.AddTransient<IMemberQuery, MemberQuery>();
            services.AddTransient<IMemberCommand, MemberCommand>();
            services.AddTransient<BlockIpToS3, BlockIpToS3>();
            services.AddTransient<IIovationReportBLL, IovationReportBLL>();
            services.AddTransient<IReferralManagementBLL, ReferralManagementBLL>();
            services.AddTransient<ICustPhotoIdBLL, CustPhotoIdBLL>();
            services.AddTransient<IVipLevelBLL, VipLevelBLL>();
            services.AddTransient<ITagBLL, TagBLL>();
            services.AddTransient<IMpaWinLossBLL, MpaWinLossBLL>();
            services.AddTransient<IMpaMarketingWithdrawalAmountBLL, MpaMarketingWithdrawalAmountBLL>();
            services.AddTransient<IMpaMarketingDepositAmountBLL, MpaMarketingDepositAmountBLL>();
            services.AddTransient<IAutoWithdrawalRuleBLL, AutoWithdrawalRuleBLL>();
            services.AddTransient<IAutoPaymentRuleBLL, AutoPaymentRuleBLL>();
            services.AddTransient<IWithdrawalStateTransferBLL, WithdrawalStateTransferBLL>();
            services.AddTransient<IWithdrawalReceiveBLL, WithdrawalReceiveBLL>();
            services.AddTransient<IWithdrawalAccountBLL, WithdrawalAccountBLL>();
            services.AddTransient<IAutoWithdrawalPaymentGateWayBLL, AutoWithdrawalPaymentGateWayBLL>();

            services.AddTransient<ReferralManagementDecorator>();
            services.AddTransient<IWorldCupPredictionService, WorldCupPredictionService>();
            services.AddTransient<ISocialNetworkService, SocialNetworkService>();
            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<ILiveRegistrationService, LiveRegistrationService>();
            services.AddTransient<IVipLevelService, VipLevelService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<IWorldCupPredictionService, WorldCupPredictionService>();
            services.AddTransient<ISocialNetworkService, SocialNetworkService>();
            services.AddTransient<IBonusReportService, BonusReportService>();
            services.AddTransient<IHotEventService, HotEventService>();
            services.AddTransient<IIovationReportService, IovationReportService>();
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<IAdminLogService, AdminLogService>();
            services.AddTransient<IMarketingService, MarketingService>();
            services.AddTransient<IAffiliateApiService, AffiliateApiService>();
            services.AddTransient<IBonusApiService, BonusApiService>();
            services.AddTransient<BetListService>();
            services.AddTransient<IMpaCommonService, MpaCommonService>();
            services.AddTransient<IMpaMarketingWithdrawalAmountService, MpaMarketingWithdrawalAmountService>();
            services.AddTransient<IMpaMarketingDepositAmountService, MpaMarketingDepositAmountService>();
            services.AddTransient<IAutoWithdrawalRuleService, AutoWithdrawalRuleService>();
            services.DecoratorReferralManagement();
            services.AddTransient<IAutoPaymentRuleService, AutoPaymentRuleService>();
            services.DecoratorLiveRegistration();
            services.AddSingleton<ConfigBLL>();
            services.AddTransient<IUploadImageBLL, UploadImageBLL>();

            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            }).AddNewtonsoftJson();
            services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            services.AddAuthentication(option =>
            {
                option.DefaultScheme = "backendV2";
            })
            .AddCookie("backendV2", "backendV2", option =>
            {
                option.LoginPath = "/login";
                option.Cookie.Name = ".backendV2";
                option.ExpireTimeSpan = TimeSpan.FromDays(1);
                option.Cookie.SameSite = SameSiteMode.Unspecified;
            });
            services.AddDataProtection().DisableAutomaticKeyGeneration();
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("CookieKeys")).SetApplicationName("SharedCookieApp");

            //Global Filter
            services.AddMvc(config =>
            {
                config.Filters.Add<ExceptionFilter>();
            });

            services.AddOpenApiDocument();//Swagger Setting

            services.AddOptionListByDB<TpGameTypeOption>("Admin_TpGameType_Get");
            services.AddOptionListByDB<TpListOption>("Common_TpList_Get");
            services.AddOptionListByDB<CurrencyOption>("Common_Currency_Get");
            services.AddOptionListByDB<Config>("SiteConfig_Get", new { ConfigGroup = "BackEndSite" });
            services.AddOptionListByDB<ServerInfo>("Admin_ServerInfoList_Get");
            services.AddSiteConfig();
            services.AddCloudFrontImageInfo();
            services.AddAffConfig();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLogMiddleware();
            app.UseResponseLogMiddleware();

            if (!env.IsEnvironment("Local"))
            {
                app.UseElasticApm(_configuration,
                    new HttpDiagnosticsSubscriber(),
                    new SqlClientDiagnosticSubscriber());
            }

            if (!env.IsProduction())
            {
                #region Swagger & Redoc Setting

                app.UseOpenApi(config =>
                {
                    config.PostProcess = (document, http) =>
                    {
                        document.Schemes = new List<OpenApiSchema>()
                        {
                            OpenApiSchema.Http,
                            OpenApiSchema.Https
                        };
                        document.Info.Title = "AdminSite API Document";
                        document.Info.Version = "2.0.8098.34336";
                    };
                });

                app.UseSwaggerUi3();

                app.UseReDoc(config =>
                {
                    config.Path = "/redoc";
                });

                #endregion

                app.UseDeveloperExceptionPage();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync($"{env.EnvironmentName}-{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });

        }
    }
}
 