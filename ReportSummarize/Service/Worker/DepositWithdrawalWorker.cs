using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ReportSummarize.Service.BLL;

namespace ReportSummarize.Service.Worker
{
    public class DepositWithdrawalWorker : BackgroundService
    {
        private readonly ILogger<DepositWithdrawalWorker> _logger;
        private readonly DepositWithdrawalBLL depositWithdrawalBLL;

        public DepositWithdrawalWorker(ILogger<DepositWithdrawalWorker> logger, DepositWithdrawalBLL depositWithdrawalBLL)
        {
            _logger = logger;
            this.depositWithdrawalBLL = depositWithdrawalBLL;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    depositWithdrawalBLL.SyncDailyReport();
                    _logger.LogInformation("DepositWithdrawalWorker done running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DepositWithdrawalWorker exception at: {DateTimeOffset.Now}, {ex.Message}");
                }
            }
        }
    }
}
