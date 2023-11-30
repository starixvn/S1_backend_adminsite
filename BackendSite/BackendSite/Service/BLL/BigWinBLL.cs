using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class BigWinBLL
    {
        private readonly BigWinService _bigWinService;

        public BigWinBLL(IServiceProvider provider)
        {
            _bigWinService = provider.GetRequiredService<BigWinService>();
        }

        public IEnumerable<BigWinSetting> GetBigWinSetting(int siteId)
        {
            return _bigWinService.GetBigWinSetting(siteId);
        }

        public async Task<bool> SetBigWinSetting(ParamsWithLog<BigWinSetting> input)
        {
            return await _bigWinService.SetBigWinSetting(input);
        }
        public IEnumerable<BigWinReport> GetBigWinReport(BigWinReportRequest request)
        {
            var currencyList = request.CurrencyId ?? new List<int>();
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;

            return _bigWinService.GetBigWinReport(currencyList, startTime, endTime);
        }
        
    }
}
