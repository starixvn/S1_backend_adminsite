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

            props.Add("PageSize", Math.Abs(prizeSelect.PageSize));
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("SelectSite", prizeSelect.SelectSite != null ? prizeSelect.SelectSite.Any()? JsonConvert.SerializeObject(prizeSelect.SelectSite) :null: null);
            props.Add("CurrencyId", prizeSelect.CurrencyId);
            props.Add("EventName", string.IsNullOrWhiteSpace(prizeSelect.EventName.Trim()) ? null : prizeSelect.EventName);

            #region GetCurrencyTotalAmount
            
            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_DrawPrizeTotalAmount_Get_V3", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            searchResult.CurrencyAmount = tempCurrencyTotalAmount;
            
            #endregion

            props.Add("TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("StartIndex", (Math.Abs(prizeSelect.PageNumber) - 1) * Math.Abs(prizeSelect.PageSize),DbType.Int32, ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(prizeSelect.PageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result = _reportDbClient.Query<DrawPrize>("Admin_DrawPrize_Get_V3", props);
            
            searchResult.DrawPrize = result.ToList();
            searchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("RowCount") ?? 0;
            
            return searchResult;
        }

        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara,int timeZone)
        {
            var logResult = new PrizeLogResult();
            var eventTime = new DateTimeOffset(Convert.ToDateTime(prizeLogPara.EventTime), TimeSpan.FromHours(timeZone)).ToUniversalTime();
            
            var props = new DynamicParameters();
            props.Add("PageNumber", Math.Abs(prizeLogPara.PageNumber),DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", (Math.Abs(prizeLogPara.PageNumber) - 1) * Math.Abs(prizeLogPara.PageSize),DbType.Int32, ParameterDirection.Input);
            props.Add("PageSize", Math.Abs(prizeLogPara.PageSize));
            props.Add("EventId", prizeLogPara.EventId);
            props.Add("PrizeId", prizeLogPara.PrizeId);
            props.Add("EventTime", eventTime.Date);
            props.Add("PrizeName", prizeLogPara.PrizeName);

            var result = _reportDbClient.Query<PrizeLogData>("Admin_DrawPrize_ListOfWinner_Get_V3", props);
            logResult.PrizeLogData = result.ToList();
            logResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            logResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            logResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return logResult;
        }

        public IEnumerable<EventDraw> GetEventName()
        {
            return _reportDbClient.Query<EventDraw>("Admin_EventDraw_Get");
        }

        public IEnumerable<DrawPrizeExcelData> GetExportData(IEnumerable<int> selectSite, string startDate, string endDate, int currencyId, string eventName, int timeZone)
        {
            var startTime = new DateTimeOffset(Convert.ToDateTime(startDate), TimeSpan.FromHours(timeZone)).ToUniversalTime();
            var endTime = new DateTimeOffset(Convert.ToDateTime(endDate), TimeSpan.FromHours(timeZone)).ToUniversalTime().AddHours(24).AddMilliseconds(-1);
            
            var props = new DynamicParameters();
            props.Add("TimeZone", TransferToTimeZoneFormat(timeZone));
            props.Add("StartDate", startTime.DateTime);
            props.Add("EndDate", endTime.DateTime);
            props.Add("SelectSite", selectSite != null ? selectSite.Any() ? JsonConvert.SerializeObject(selectSite) : null : null);
            props.Add("CurrencyId", currencyId);
            props.Add("EventName", string.IsNullOrWhiteSpace(eventName) ? null : eventName);

            return _reportDbClient.Query<DrawPrizeExcelData>("Admin_DrawPrizeExport_Get_V2", props);

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
