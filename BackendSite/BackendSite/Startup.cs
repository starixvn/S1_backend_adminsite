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
using NSwag;
using System.Collections.Generic;
using BackendSite.Service.Model;
using StackExchange.Redis;

namespace BackendSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetValue<string>("RedisConnection")));
            services.AddSingleton<ICacheHelper, RedisCacheHelper>();
            /*Attributes*/
            services.AddSingleton<AuthorizeByFuncAttribute>();

            /*Library*/
            services.AddSingleton<Tools>();
            services.AddSingleton<Common>();
            services.AddSingleton<CourierAdapter>();
            services.AddSingleton<CodeGenerator>();
            services.AddSingleton<CacheHelper>();
            services.AddDatabaseConnection(Configuration);
            services.Configure<Dictionary<string, SiteConfig>>(Configuration.GetSection("SiteConfig"));
            services.AddRedisConnection(Configuration);
            services.AddSingleton<RedisHelper>();

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
            })
            .AddCookie("brand", "brand", option =>
            {
                option.LoginPath = "/Home/AuthorizedFailed";
                option.Cookie.Name = ".brand";
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
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
