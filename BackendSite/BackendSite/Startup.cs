using System;
using System.IO;
using System.Linq;
using BackendSite.Service.Library;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BackendSite.Attributes;
using BackendSite.Extensions;
using Microsoft.AspNetCore.Http;
using BackendSite.ExceptionHandling;

namespace BackendSite
{
    public class Startup
    {
        private readonly string AllowSpecificOrigins = "AllowAllOriginsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            /*Attributes*/
            services.AddSingleton<AuthorizeByFuncAttribute>();

            /*Library*/
            services.AddSingleton<Tools>();
            services.AddSingleton<Common>();
            services.AddSingleton<CourierAdapter>();
            services.AddSingleton<CodeGenerator>();
            services.AddDatabaseConnection(Configuration);

            /*DAL*/
            //services.AddSingleton<ServerInfoService>();
            //services.AddSingleton<PendingFundService>();
            //services.AddSingleton<AdminService>();
            //services.AddSingleton<DepositService>();
            //services.AddSingleton<WithdrawalService>();
            //services.AddSingleton<WalletService>();
            //services.AddSingleton<BankDetailService>();
            //services.AddSingleton<AdminUserService>();
            //services.AddSingleton<MessageService>();
            //services.AddSingleton<MenuService>();
            //services.AddSingleton<CustInfoService>();
            //services.AddSingleton<LocalBankService>();
            //services.AddSingleton<AdjustmentService>();
            //services.AddSingleton<ThirdPartyService>();
            //services.AddSingleton<MarketingService>();
            //services.AddSingleton<RollOverService>();
            //services.AddSingleton<WithdrawalLimitService>();
            //services.AddSingleton<PromotionService>();
            //services.AddSingleton<PermissionService>();
            //services.AddSingleton<AdminGroupService>();
            //services.AddSingleton<WinLostReportService>();
            //services.AddSingleton<PaymentAdjustmentReportService>();
            //services.AddSingleton<BonusInfoService>();
            //services.AddSingleton<PaymentManageService>();
            //services.AddSingleton<CustTurnoverService>();
            //services.AddSingleton<BonusCashBackService>();
            //services.AddSingleton<HomeService>();
            //services.AddSingleton<BonusManageService>();
            //services.AddSingleton<NewsServices>();
            //services.AddSingleton<RebateManageService>();
            //services.AddSingleton<ReportService>();
            //services.AddSingleton<BannerManageService>();
            //services.AddSingleton<ReferralManagementService>();
            //services.AddSingleton<SportsService>();
            //services.AddSingleton<PrizeService>();
            //services.AddSingleton<RebateManageService>(); //���ƤF...

            //services.AddSingleton<VipLevelService>();
            //services.AddSingleton<GameCasinoManageService>();
            //services.AddSingleton<BankFinancialService>();
            //services.AddSingleton<LatestHighlightVideoManagementService>();
            //services.AddSingleton<DomainTableService>();
            //services.AddSingleton<ReasonManagementService>();
            //services.AddSingleton<DashBoardService>();

            //var dal = AppDomain.CurrentDomain.GetAssemblies()
            //           .SelectMany(t => t.GetTypes())
            //           .Where(t => t.IsClass && t.Namespace == "BackendSite.Service.DAL");

            //foreach (var s in dal)
            //{
            //    services.AddSingleton(s);
            //}

            var daltest = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in daltest)
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
            //services.AddSingleton<ServerInfoBLL>();
            //services.AddSingleton<PendingFundBLL>();
            //services.AddSingleton<AdminBLL>();
            //services.AddSingleton<SelLangBLL>();
            //services.AddSingleton<DepositBLL>();
            //services.AddSingleton<WithdrawalBLL>();
            //services.AddSingleton<BankDetailBLL>();
            //services.AddSingleton<MessageBLL>();
            //services.AddSingleton<AuthorizeBLL>();
            //services.AddSingleton<CustInfoBLL>();
            //services.AddSingleton<MarketingBLL>();
            //services.AddSingleton<PaymentBLL>();
            //services.AddSingleton<TpBLL>();
            //services.AddSingleton<ExcelBLL>();
            //services.AddSingleton<AdminGroupBLL>();
            //services.AddSingleton<PermissionBLL>();
            //services.AddSingleton<AdminUserBLL>();
            //services.AddSingleton<MenuBLL>();
            //services.AddSingleton<WinLostReportBLL>();
            //services.AddSingleton<PaymentAdjustmentReportBLL>();
            //services.AddSingleton<BonusInfoBLL>();
            //services.AddSingleton<PaymentManageBLL>();
            //services.AddSingleton<AdjustWithdrawalLimitBLL>();
            //services.AddSingleton<BonusCashBackBLL>();
            //services.AddSingleton<HomeBLL>();
            //services.AddSingleton<BonusManageBLL>();
            //services.AddSingleton<NewsBLL>();
            //services.AddSingleton<RebateManageBLL>();

            //services.AddSingleton<ReportBLL>();
            //services.AddSingleton<BannerManageBLL>();
            //services.AddSingleton<ReportExcelBLL>();
            //services.AddSingleton<ReferralManagementBLL>();
            //services.AddSingleton<SportsBLL>();
            //services.AddSingleton<PrizeBLL>();
            ////services.AddSingleton<RebateManageBLL>(); //���ƤF...
            //services.AddSingleton<VipLevelBLL>();
            //services.AddSingleton<GameCasinoManageBLL>();
            //services.AddSingleton<BankFinancialBLL>();
            //services.AddSingleton<LatestHighlightVideoManagementBLL>();
            //services.AddSingleton<DomainTableBLL>();
            //services.AddSingleton<ReasonManagementBLL>();
            //services.AddSingleton<DashBoardBLL>();
            //services.AddSingleton<ResponseSettingBLL>();

            //var bll = AppDomain.CurrentDomain.GetAssemblies()
            //           .SelectMany(t => t.GetTypes())
            //           .Where(t => t.IsClass && t.Namespace == "BackendSite.Service.BLL");

            // foreach (var s in bll)
            // {
            //     services.AddSingleton(s);
            // }

            var test = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in test)
            {
                var list = item.DefinedTypes.Where(t => t.IsClass
                                                        && t.Namespace == "BackendSite.Service.BLL"
                                                        && t.Name.Contains("BLL")).ToList();

                list.ForEach((type) =>
                {
                    services.AddSingleton(type, type);
                });

            }

            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            }).AddNewtonsoftJson();
            services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            services.AddHttpContextAccessor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
            {
                //option.AccessDeniedPath
                option.LoginPath = "/Home/Signin";
                option.Cookie.Name = ".backend";
                option.ExpireTimeSpan = TimeSpan.FromDays(1);//?S???w?]14??
                option.Cookie.SameSite = SameSiteMode.Unspecified;
            });
            services.AddDataProtection().DisableAutomaticKeyGeneration();
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("CookieKeys")).SetApplicationName("SharedCookieApp");

            #region Global Filter

            services.AddMvc(config =>
            {
                config.Filters.Add<ExceptionFilter>();
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(AllowSpecificOrigins);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLogMiddleware();
            app.UseResponseLogMiddleware();

            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });

        }
    }
}
