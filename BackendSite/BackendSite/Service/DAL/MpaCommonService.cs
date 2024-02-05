using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaCommonService : IMpaCommonService
    {
        private readonly MySqlClient _reportDbClient;
        private readonly ICacheHelper _redisHelper;

        public MpaCommonService(IEnumerable<MySqlClient> sqlClients, ICacheHelper redisHelper)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<MpaRateDto>> GetMpaExchangeRate(DateTimeOffset starTime, DateTimeOffset endTime, int baseCurrencyId = 0)
        {
            var redisKey = $"MpaExchangeRate:{starTime:yyyy-MM-dd HH:mm:ss}:{endTime:yyyy-MM-dd HH:mm:ss}:{baseCurrencyId}";

            if (baseCurrencyId == 0)
            {
                var exchangeRate = await _redisHelper.TryGetOrCreateAsync(redisKey,
                    TimeSpan.FromMinutes(10),
                    () => GetRateFromDb(starTime, endTime));

                return exchangeRate;
            }
            else
            {
                var exchangeRate = await _redisHelper.TryGetOrCreateAsync(redisKey,
                    TimeSpan.FromMinutes(10),
                    async () => (await GetRateFromDb(starTime, endTime)).Where(x => x.TargetCurrencyId == baseCurrencyId));

                return exchangeRate;
            }
        }

        public decimal MapperExchangeRate(IEnumerable<MpaRateDto> source, DateTime date, int currencyId, int baseCurrencyId)
        {
            //是否是當天
            if (date == DateTime.Today)
            {
                return source
                    .FirstOrDefault(r => r.RateDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd")
                                         && r.CurrencyId == currencyId
                                         && r.TargetCurrencyId == baseCurrencyId)!.Rate;
            }

            //是否是當月
            if (date.ToString("yyyy-MM") == DateTime.Now.ToString("yyyy-MM"))
            {
                return source
                    .Where(r => r.RateDate.ToString("yyyy-MM") == DateTime.Now.ToString("yyyy-MM")
                                && r.CurrencyId == currencyId
                                && r.TargetCurrencyId == baseCurrencyId).MaxBy(r => r.RateDate)!.Rate;
            }


            //抓同年同月的最後一筆匯率
            var lastDay = date.AddMonths(1).AddDays(-date.AddMonths(1).Day).ToString("yyyy-MM-dd");
            return source.FirstOrDefault(r => r.RateDate.ToString("yyyy-MM-dd") == lastDay
                                              && r.CurrencyId == currencyId
                                              && r.TargetCurrencyId == baseCurrencyId)!.Rate;
        }

        public async Task<Dictionary<int, string>> GetCurrencyIsoCodeMapper()
        {
            const string redisKey = "MpaCurrencyInfo";
            var data = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12), GetMpaCurrencyInfo);

            var mapper = data.ToDictionary(k => k.CurrencyId, v => v.IsoCode);

            return mapper;
        }

        public string GetPercent(decimal divisor, decimal dividend)
        {
            decimal percent;

            if (divisor < dividend)
            {
                percent = 1 - (divisor / dividend);
                percent *= -1;
            }
            else
            {
                percent = 1 - (dividend / divisor);
            }

            var result = percent.ToString("0%");
            return result;
        }

        public string RegisteredDateFormatter(DateTime registeredDate)
        {
            var originRegisteredDate = registeredDate.ToString("dd-MM-yyyy hh:mm:ss");
            var originMonthNum = originRegisteredDate.Substring(3, 2);

            var monthText = originMonthNum switch
            {
                "01" => "Jan",
                "02" => "Feb",
                "03" => "Mar",
                "04" => "Apr",
                "05" => "May",
                "06" => "jun",
                "07" => "Jul",
                "08" => "Aug",
                "09" => "Sep",
                "10" => "Oct",
                "11" => "Nov",
                "12" => "Dec",
                _ => string.Empty
            };

            return $"{originRegisteredDate.Substring(0, 3)}{monthText}{originRegisteredDate.Substring(5, originRegisteredDate.Length - 5)}";
        }

        public Dictionary<TKey, TValue> SortApiData<TKey, TValue>(Dictionary<TKey, TValue> exportData, string sortField, OrderByDirection sortOrder)
        {
            if (sortField == "key")
            {
                if (sortOrder == OrderByDirection.Ascending)
                {
                    return exportData.OrderBy(kvp => kvp.Key)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                return exportData.OrderByDescending(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var propertyNames = typeof(TValue).GetProperties().Where(x => string.Equals(x.Name, sortField, StringComparison.CurrentCultureIgnoreCase));
            var propertyName = propertyNames.Select(x => x.Name).FirstOrDefault() ?? "";

            var propertyInfo = typeof(TValue).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException("Invalid sort field or type.");
            }

            if (sortOrder == OrderByDirection.Ascending)
            {
                return exportData.OrderBy(kvp => propertyInfo.GetValue(kvp.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return exportData.OrderByDescending(kvp => propertyInfo.GetValue(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private Task<IEnumerable<MpaRateDto>> GetRateFromDb(DateTimeOffset starTime, DateTimeOffset endTime)
        {
            var p = new DynamicParameters();
            p.Add("_RateDateStart", starTime.Date.AddDays(-1), dbType: DbType.DateTime);
            p.Add("_RateDateEnd", endTime.Date.AddMonths(1), dbType: DbType.DateTime);

            return _reportDbClient.QueryAsync<MpaRateDto>("Admin_ExchangeRate_Get", p);
        }

        private Task<IEnumerable<MpaCurrencyDto>> GetMpaCurrencyInfo()
        {
            return _reportDbClient.QueryAsync<MpaCurrencyDto>("Admin_CurrencyInfo_Get");
        }
    }
}