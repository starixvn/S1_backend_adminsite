using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class BigWinBLL
    {
        private readonly BigWinService _bigWinService;
        private readonly ICacheHelper _cacheHelper;
        private readonly SlotGamesManagementService _slotGamesManagementService;

        public BigWinBLL(IServiceProvider provider)
        {
            _bigWinService = provider.GetRequiredService<BigWinService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _slotGamesManagementService = provider.GetRequiredService<SlotGamesManagementService>();
        }

        public IEnumerable<BigWinSetting> GetBigWinSetting(int siteId)
        {
            return _bigWinService.GetBigWinSetting(siteId);
        }

        public async Task<bool> SetBigWinSetting(ParamsWithLog<BigWinSetting> input)
        {
            return await _bigWinService.SetBigWinSetting(input);
        }
        public IEnumerable<BigWinExportData> GetBigWinReport(BigWinReportRequest request)
        {
            var result = new List<BigWinExportData>();
            var currencyList = request.CurrencyId ?? new List<int>();
            var startTime = request.StartDate.ToUniversalTime().DateTime.Date;
            var endTime = request.EndDate.ToUniversalTime().DateTime.Date;

            var bigWinReportList = _bigWinService.GetBigWinReport(currencyList, startTime, endTime, request.SiteId);

            foreach (var bigWinReport in bigWinReportList)
            {
                var gameName = bigWinReport.TypeName;

                if (bigWinReport.TpCategory == (int)TpCategoryEnum.Casino)
                {
                    var key = $"GetSlotGameList:{bigWinReport.SiteId}:{bigWinReport.CurrencyId}";
                    var slotGameList = _cacheHelper.TryGetOrCreate(key, TimeSpan.FromHours(1),
                        () => _slotGamesManagementService.GetSlotGameList(bigWinReport.SiteId, bigWinReport.CurrencyId));
                    var gameSlotDetail = slotGameList.FirstOrDefault(i => i.TpId == bigWinReport.TpId && (i.GameId == bigWinReport.GameID || i.AppId == bigWinReport.GameID));

                    if (gameSlotDetail != null)
                    {
                        gameName = gameSlotDetail.GameName.ContainsKey("en") ? gameSlotDetail.GameName["en"] : string.Empty;
                    }
                }
                 
                result.Add(new BigWinExportData() 
                {
                    Site = bigWinReport.Site,
                    Currency = bigWinReport.Currency,
                    EventDate = bigWinReport.EventDate,
                    MemberID = bigWinReport.MemberID,
                    Category = bigWinReport.Category,
                    VenderName = bigWinReport.VenderName,
                    GameName = gameName,
                    Amount = bigWinReport.Amount,
                });
            }

            return result;
        }

    }
}
