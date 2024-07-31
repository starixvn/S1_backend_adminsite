using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PrizeService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public PrizeService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public DrawPrizeReportResult PrizeSearch(PrizeSelect prizeSelect, int timeZone, out List<string> currencyAmount)
        {
            var searchResult = new DrawPrizeReportResult();
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();

            props.Add("_StartDate", $"{prizeSelect.StartDate} 00:00:00");
            props.Add("_EndDate", $"{prizeSelect.EndDate} 23:59:59");
            props.Add("_SiteIds", JsonConvert.SerializeObject(prizeSelect.SelectSite));
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(prizeSelect.CurrencyId));
            props.Add("_EventNames", JsonConvert.SerializeObject(prizeSelect.EventName));

            #region GetCurrencyTotalAmount

            searchResult.CurrencyAmount = _reportDbClient.Query<DrawPrizeReportOverviewResult>("Admin_DrawPrizeTotalAmount_Get_V5", props).ToList();
            if (searchResult.CurrencyAmount.Count > 0)
            {
                searchResult.CurrencyAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.CurrencyName, Math.Round(x.TotalAmount, 2)));
                });
            }
            currencyAmount = tempCurrencyTotalAmount;

            #endregion

            props.Add("_PageSize", Math.Abs(prizeSelect.PageSize));
            props.Add("_TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("_StartIndex", (Math.Abs(prizeSelect.PageNumber) - 1) * Math.Abs(prizeSelect.PageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<DrawPrize>("Admin_DrawPrize_Get_V5", props);

            searchResult.Data = result.ToList();
            searchResult.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return searchResult;
        }

        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara)
        {
            var logResult = new PrizeLogResult();
      
            var props = new DynamicParameters();
            props.Add("_EventId", prizeLogPara.EventId);
            props.Add("_PrizeId", prizeLogPara.PrizeId);
            props.Add("_EventTime", prizeLogPara.EventTime.DateTime);
            props.Add("_PrizeName", prizeLogPara.PrizeName);

            props.Add("_StartIndex", (Math.Abs(prizeLogPara.PageNumber) - 1) * Math.Abs(prizeLogPara.PageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("_PageSize", Math.Abs(prizeLogPara.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<PrizeLogData>("Admin_DrawPrize_ListOfWinner_Get_V5", props);
            logResult.PrizeLogData = result.ToList();
            logResult.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            logResult.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return logResult;
        }

        public IEnumerable<EventDraw> GetEventName()
        {
            return _reportDbClient.Query<EventDraw>("Admin_EventDraw_Get");
        }

        public IEnumerable<DrawPrizeExcelData> GetExportData(string selectSite, string startDate, string endDate, string currencyId, string eventName, int timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("_StartDate", $"{startDate} 00:00:00.000");
            props.Add("_EndDate", $"{endDate} 23:59:59.999");
            props.Add("_SiteIds", selectSite);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_EventNames", eventName);

            return _reportDbClient.Query<DrawPrizeExcelData>("Admin_DrawPrizeExport_Get_V4", props);
        }

        private string TransferToTimeZoneFormat(int timeZone)
        {
            if (Math.Sign(timeZone) > 0 || Math.Sign(timeZone) == 0)
            {
                return string.Format("+" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
            else
            {
                return string.Format("-" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
        }
    }
}
