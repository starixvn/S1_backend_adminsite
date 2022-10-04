using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public partial class DashboardBLL
{
    private readonly ReportBLL _reportBll;
    private readonly TpMaintainBLL _tpMaintainBll;
    private readonly DashBoardService dashBoardService;

    public DashboardBLL(IServiceProvider provider)
    {
        dashBoardService = provider.GetRequiredService<DashBoardService>();
        _reportBll = provider.GetRequiredService<ReportBLL>();
        _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
    }

    public DashBoardGameDto GetGameData(IEnumerable<int> siteIds,
                               IEnumerable<int> currencyIds,
                               int baseCurrencyId,
                               IEnumerable<int> tpIds,
                               IEnumerable<int> categoryIds,
                               IEnumerable<int> gameTypes,
                               int timeZone)
    {
        var result = new DashBoardGameDto();
        var pieChartSummary = new Dictionary<string, List<DashBoardGameSimpleSummaryDto>>();
        var lineChartPerMonthSummary = new List<DashBoardGamePerMonthDto>();

        //Step1:決定日期區間  

        var dateTimeNowInTimeZone = DateTime.UtcNow.AddHours(timeZone);

        //Step2:撈資料
        for (int i = 1; i <= dateTimeNowInTimeZone.Month; i++)
        {
            var dateRange = CaculateDateRange(dateTimeNowInTimeZone.Year, i, timeZone);
            var sourceData = GetGameSourceDataPerMonth(dateTimeNowInTimeZone.Year, i,
                siteIds, currencyIds, baseCurrencyId,
                dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);
            var sourceMemberData = GetDashBoardGameUniqueMemberDataPerMonth(dateTimeNowInTimeZone.Year, i,
              siteIds, currencyIds, baseCurrencyId,
              dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);

            var gameData = GetGameDataPerMonth(dateTimeNowInTimeZone.Year, i,
                currencyIds, baseCurrencyId, sourceData, sourceMemberData);

            CombinePieChartSummary(sourceData, ref pieChartSummary);
            lineChartPerMonthSummary.AddRange(gameData);
        }

        result.PieChartSummary = pieChartSummary;
        result.LineChartPerMonthSummary = lineChartPerMonthSummary;

        //Step3:組合資料
        return result;
    }

    private (DateTimeOffset StartDate, DateTimeOffset EndDate) CaculateDateRange(int year, int month, int timeZone)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        return (StartDate: new DateTimeOffset(firstDayOfMonth, new TimeSpan(timeZone, 0, 0)),
                EndDate: new DateTimeOffset(lastDayOfMonth, new TimeSpan(timeZone, 0, 0)));
    }

    private IEnumerable<DashBoardGamePerMonthDao> GetGameSourceDataPerMonth(int year, int month,
                                       IEnumerable<int> siteIds,
                                       IEnumerable<int> currencyIds,
                                       int baseCurrencyId,
                                       DateTimeOffset startTime,
                                       DateTimeOffset endTime,
                                       IEnumerable<int> tpIds,
                                       IEnumerable<int> categoryIds,
                                       IEnumerable<int> gameTypes)

    => dashBoardService.GetDashBoardGameData(JsonConvert.SerializeObject(siteIds),
       JsonConvert.SerializeObject(currencyIds),
       startTime.DateTime,
       endTime.DateTime,
       JsonConvert.SerializeObject(tpIds),
       JsonConvert.SerializeObject(categoryIds),
       JsonConvert.SerializeObject(gameTypes));

    private IEnumerable<DashBoardGamePerMonthDao> GetDashBoardGameUniqueMemberDataPerMonth(int year, int month,
                                   IEnumerable<int> siteIds,
                                   IEnumerable<int> currencyIds,
                                   int baseCurrencyId,
                                   DateTimeOffset startTime,
                                   DateTimeOffset endTime,
                                   IEnumerable<int> tpIds,
                                   IEnumerable<int> categoryIds,
                                   IEnumerable<int> gameTypes)
    => dashBoardService.GetDashBoardGameUniqueMemberData(JsonConvert.SerializeObject(siteIds),
       JsonConvert.SerializeObject(currencyIds),
       startTime.DateTime,
       endTime.DateTime,
       JsonConvert.SerializeObject(tpIds),
       JsonConvert.SerializeObject(categoryIds),
       JsonConvert.SerializeObject(gameTypes));

    private void CombinePieChartSummary(IEnumerable<DashBoardGamePerMonthDao> sourceData,
        ref Dictionary<string, List<DashBoardGameSimpleSummaryDto>> pieChartSummary)
    {
        var result = new Dictionary<string, List<DashBoardGameSimpleSummaryDto>>();

        var tpProviderMappingResult = MappingTpProvieder(sourceData);
        var pieChartData = pieChartSummary[tpProviderMappingResult.Key].Select(x => x.Id);
        foreach (var item in tpProviderMappingResult.Value)
        {
            if (pieChartData.Contains(item.Id))
                pieChartSummary[tpProviderMappingResult.Key].FirstOrDefault(x => x.Id == item.Id).Count
                    += tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id).Count;
            else
                pieChartSummary[tpProviderMappingResult.Key].Add(tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id));
        }
        var tpCategoryMappingResult = MappingTpCategory(sourceData).Result;
        foreach (var item in tpCategoryMappingResult.Value)
        {
            if (pieChartData.Contains(item.Id))
                pieChartSummary[tpCategoryMappingResult.Key].FirstOrDefault(x => x.Id == item.Id).Count
                    += tpCategoryMappingResult.Value.FirstOrDefault(x => x.Id == item.Id).Count;
            else
                pieChartSummary[tpCategoryMappingResult.Key].Add(tpCategoryMappingResult.Value.FirstOrDefault(x => x.Id == item.Id));
        }
    }

    private (string Key, List<DashBoardGameSimpleSummaryDto> Value) MappingTpProvieder(IEnumerable<DashBoardGamePerMonthDao> sourceData)
    {
        var result = new List<DashBoardGameSimpleSummaryDto>();
        var tpList = _tpMaintainBll.GetCommonTpList();
        var existTpList = sourceData.Select(x => x.TpId).Distinct().OrderBy(x => x);

        foreach (var tpId in existTpList)
            result.Add(new DashBoardGameSimpleSummaryDto()
            {
                Id = tpId,
                Name = tpList.FirstOrDefault(x => x.Id == tpId).TpName,
                Count = sourceData.Where(x => x.TpId == tpId).Sum(x => x.CountSum)
            });

        return (Key: "Game Provider", Value: result);
    }

    private async Task<(string Key, List<DashBoardGameSimpleSummaryDto> Value)> MappingTpCategory(IEnumerable<DashBoardGamePerMonthDao> sourceData)
    {
        var result = new List<DashBoardGameSimpleSummaryDto>();
        var categoryList = await _reportBll.GetTpCategoryList();
        var existCategoryList = sourceData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);

        foreach (var categoryId in existCategoryList)
            result.Add(new DashBoardGameSimpleSummaryDto()
            {
                Id = categoryId,
                Name = categoryList.FirstOrDefault(x => x.CategoryId == categoryId).CategoryName,
                Count = sourceData.Where(x => x.TpId == categoryId).Sum(x => x.CountSum)
            });

        return (Key: "Game Category", Value: result);
    }

    private List<DashBoardGamePerMonthDto> GetGameDataPerMonth(int year, int month, IEnumerable<int> currencyIds,
        int baseCurrencyId, IEnumerable<DashBoardGamePerMonthDao> sourceData, IEnumerable<DashBoardGamePerMonthDao> memberSourceData)
    {
        var result = new List<DashBoardGamePerMonthDto>();
        var currencyItem = new DashboardGamePerMonthCurrencyDto();
        var currencyList = _reportBll.GetCurrency();

        foreach (var currencyId in currencyIds)
        {
            var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);
            var currencyData = sourceData.Where(x => x.CurrencyId == currencyId);
            var currencyMemberData = memberSourceData.Where(x => x.CurrencyId == currencyId);
            var currencyDetail = new DashBoardGamePerMonthDto()
            {
                Year = year,
                Month = month,
                CurrencyId = currencyId,
                CurrencyName = currencyInfo.IsoCode,
                CurrencyDetail = new CurrencyPerMonthDto()
                {
                    BetCount = currencyData.Sum(x => x.CountSum),
                    UniqueMember = memberSourceData.Count(),
                    OriginalCurrencyData = new DashboardGamePerMonthCurrencyDto()
                    {
                        BetAmount = currencyData.Sum(x => x.ValidBetSum),
                        AverageBetAmount = currencyData.Sum(x => x.ValidBetSum) / currencyData.Sum(x => x.CountSum),
                        Payout = currencyData.Sum(x => x.WinLossSum) + currencyData.Sum(x => x.ValidBetSum),
                        GGR = currencyData.Where(x => x.CategoryId == 8).Sum(x => x.RevenueSum)
                        + (currencyData.Where(x => x.CategoryId != 8).Sum(x => x.WinLossSum) * -1)
                    }
                }
            };
            currencyDetail.CurrencyDetail.Margin =
                currencyDetail.CurrencyDetail.OriginalCurrencyData.GGR / currencyDetail.CurrencyDetail.OriginalCurrencyData.BetAmount;

            if (baseCurrencyId != 0)
            {
                var currencyBaseInfo = currencyList.FirstOrDefault(x => x.CurrencyId == baseCurrencyId);

                currencyDetail.CurrencyDetail.BaseCurrencyData = new DashboardGamePerMonthCurrencyDto()
                {
                    BetAmount = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.BetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                    AverageBetAmount = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.AverageBetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                    Payout = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.Payout, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                    GGR = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate)
                };
            }
            else
                currencyDetail.CurrencyDetail.BaseCurrencyData = currencyDetail.CurrencyDetail.OriginalCurrencyData;

            result.Add(currencyDetail);
        }
        return result;
    }

    private decimal ChangeRate(decimal originalAmount, decimal originalRate, decimal baseRate)
    => originalAmount * originalRate / baseRate;

}
