using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

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

        public PrizeSearchResult PrizeSearch(PrizeSelect prizeSelect, int timeZone)
        {
            var searchResult = new PrizeSearchResult();
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();
            var startDate = new DateTimeOffset(Convert.ToDateTime(prizeSelect.StartDate), TimeSpan.FromHours(timeZone)).ToUniversalTime();
            var endDate = new DateTimeOffset(Convert.ToDateTime(prizeSelect.EndDate), TimeSpan.FromHours(timeZone)).ToUniversalTime().AddHours(24).AddMilliseconds(-1);

            props.Add("_StartDate", startDate.DateTime);
            props.Add("_EndDate", endDate.DateTime);
            props.Add("_SiteIds", JsonConvert.SerializeObject(prizeSelect.SelectSite));
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(prizeSelect.CurrencyId));
            props.Add("_EventNames", JsonConvert.SerializeObject(prizeSelect.EventName));

            #region GetCurrencyTotalAmount
            
            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_DrawPrizeTotalAmount_Get_V4", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            searchResult.CurrencyAmount = tempCurrencyTotalAmount;

            #endregion

            props.Add("_PageSize", Math.Abs(prizeSelect.PageSize));
            props.Add("_TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("_StartIndex", (Math.Abs(prizeSelect.PageNumber) - 1) * Math.Abs(prizeSelect.PageSize),DbType.Int32, ParameterDirection.Input);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result = _reportDbClient.Query<DrawPrize>("Admin_DrawPrize_Get_V4", props);
            
            searchResult.DrawPrize = result.ToList();
            searchResult.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("_RowCount") ?? 0;
            
            return searchResult;
        }

        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara,int timeZone)
        {
            var logResult = new PrizeLogResult();
            var eventTime = new DateTimeOffset(Convert.ToDateTime(prizeLogPara.EventTime), TimeSpan.FromHours(timeZone)).ToUniversalTime();
            
            var props = new DynamicParameters();
            props.Add("_EventId", prizeLogPara.EventId);
            props.Add("_PrizeId", prizeLogPara.PrizeId);
            props.Add("_EventTime", eventTime.Date);
            props.Add("_PrizeName", prizeLogPara.PrizeName);

            props.Add("_StartIndex", (Math.Abs(prizeLogPara.PageNumber) - 1) * Math.Abs(prizeLogPara.PageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("_PageSize", Math.Abs(prizeLogPara.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<PrizeLogData>("Admin_DrawPrize_ListOfWinner_Get_V4", props);
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
            var startTime = new DateTimeOffset(Convert.ToDateTime(startDate), TimeSpan.FromHours(timeZone)).ToUniversalTime();
            var endTime = new DateTimeOffset(Convert.ToDateTime(endDate), TimeSpan.FromHours(timeZone)).ToUniversalTime().AddHours(24).AddMilliseconds(-1);
            
            var props = new DynamicParameters();
            props.Add("_TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("_StartDate", startTime.DateTime);
            props.Add("_EndDate", endTime.DateTime);
            props.Add("_SiteIds", selectSite);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_EventNames", eventName);

            return _reportDbClient.Query<DrawPrizeExcelData>("Admin_DrawPrizeExport_Get_V3", props);

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
