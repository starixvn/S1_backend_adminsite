using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportSummarize.Service.Worker;
using ReportSummarize.Service.BLL;
using ReportSummarize.Service.DAL;

namespace ReportSummarize
{
    public class Program
    {
        public IConfiguration Configuration { get; }
        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd() // for linux
                .UseWindowsService() // for windows
                .ConfigureServices((hostContext, services) =>
                {
                    //Worker
                    services.AddHostedService<WinLostWorker>();
                    services.AddHostedService<DepositWithdrawalWorker>();

                    //BLL
                    services.AddSingleton<DepositWithdrawalBLL>();
                    services.AddSingleton<WinLostBLL>();

                    //Service
                    services.AddSingleton<DepositWithdrawalService>();
                    services.AddSingleton<WinLostService>();
                });
    }
}
