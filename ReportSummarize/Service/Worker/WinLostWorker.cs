using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ReportSummarize.Service.BLL;

namespace ReportSummarize.Service.Worker
{
    public class WinLostWorker : BackgroundService
    {
        private readonly ILogger<WinLostWorker> _logger;
        private readonly WinLostBLL winLostBLL;

        public WinLostWorker(ILogger<WinLostWorker> logger, WinLostBLL winLostBLL)
        {
            _logger = logger;
            this.winLostBLL = winLostBLL;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    winLostBLL.SyncDailyReport();
                    _logger.LogInformation("WinLostWorker done running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"WinLostWorker exception at: {DateTimeOffset.Now}, {ex.Message}");
                }
            }
        }
    }
}
