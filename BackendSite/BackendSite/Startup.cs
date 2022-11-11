using BackendSite.Service.Library;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.ExceptionHandling;
using NSwag;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using StackExchange.Redis;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

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
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(_configuration.GetValue<string>("RedisConnection")));
            services.AddSingleton<ICacheHelper, RedisCacheHelper>();
            /*Attributes*/
            services.AddSingleton<AuthorizeByFuncAttribute>();

            /*Library*/
            services.AddSingleton<Tools>();
            services.AddSingleton<Common>();
            services.AddSingleton<CourierAdapter>();
            services.AddSingleton<CodeGenerator>();
            services.AddSingleton<CacheHelper>();
            services.AddDatabaseConnection(_configuration);
            services.Configure<Dictionary<string, SiteConfig>>(_configuration.GetSection("SiteConfig"));          
            services.Configure<Dictionary<string, AWSKeyConfig>>(_configuration.GetSection("AwsKeyConfig"));
            services.AddSingleton<AWSS3Helper>();
            services.AddSingleton<AWSCloudFrontHelper>();

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

            services.AddTransient<BlockIpToS3, BlockIpToS3>();
            services.AddTransient<IReferralManagementBLL, ReferralManagementBLL>();
            services.AddTransient<ILiveRegistrationBLL, LiveRegistrationBLL>();
            services.AddTransient<ReferralManagementDecorator>();
            services.AddTransient<LiveRegistrationDecorator>();
            services.AddTransient<IWorldCupPredictionService, WorldCupPredictionService>();
            services.AddTransient<ISocialNetworkService, SocialNetworkService>();
            services.AddTransient<IBonusReportService, BonusReportService>();
            services.AddTransient<IHotEventService, HotEventService>();
            services.AddTransient<IMpaWinLossService, MpaWinLossService>();
            services.AddTransient<IMpaWinLossBLL, MpaWinLossBLL>();

            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            }).AddNewtonsoftJson();
            services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            services.AddHttpContextAccessor();
            services.AddAuthentication(option => 
            {
                option.DefaultScheme = "backend";
            })
            .AddCookie("backend", "backend", option =>
            {
                //option.AccessDeniedPath
                option.LoginPath = "/Home/Signin";
                option.Cookie.Name = ".backend";
                option.ExpireTimeSpan = TimeSpan.FromDays(1);//?S???w?]14??
                option.Cookie.SameSite = SameSiteMode.Unspecified;
            })
            .AddCookie("backendV2", "backendV2", option =>
            {
                option.LoginPath = "/Home/AuthorizedFailed";
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
