using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using Dapper;
using System.Data;

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

        public async Task<IEnumerable<MpaRateDto>> GetMpaExchangeRate(DateTimeOffset starTime, DateTimeOffset endTime)
        {
            var redisKey = $"MpaExchangeRate:{starTime:yyyy-MM-dd HH:mm:ss}:{endTime:yyyy-MM-dd HH:mm:ss}";

            var exchangeRate = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => GetRateFromDb(starTime,endTime));

            return exchangeRate;
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

        private Task<IEnumerable<MpaRateDto>> GetRateFromDb(DateTimeOffset starTime, DateTimeOffset endTime)
        {
            var p = new DynamicParameters();
            p.Add("_RateDateStart", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_RateDateEnd", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);

            return _reportDbClient.QueryAsync<MpaRateDto>("Admin_ExchangeRate_Get", p);
        }

        private Task<IEnumerable<MpaCurrencyDto>> GetMpaCurrencyInfo()
        {
            return _reportDbClient.QueryAsync<MpaCurrencyDto>("Admin_CurrencyInfo_Get");
        }
    }
}