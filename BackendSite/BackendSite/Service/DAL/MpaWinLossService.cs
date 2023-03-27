using System.Data;
using System.Diagnostics;
using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaWinLossService: IMpaWinLossService
    {
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<MpaWinLossService> _logger;

        public MpaWinLossService(IEnumerable<MySqlClient> sqlClients, ILogger<MpaWinLossService> logger)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
            _logger = logger;
        }

        public Task<IEnumerable<TResult>> GetMapTpBetRecord<TResult>(int siteId, DateTimeOffset starTime, DateTimeOffset endTime, string format,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_DateType", (int)mpaDateType, dbType: DbType.Int32);

            var sw = Stopwatch.StartNew();
            sw.Start();
            var data = _reportDbClient.QueryAsync<TResult>("Admin_TpBetRecordRelayGroupByDate_Get", p);
            sw.Stop();
            _logger.LogInformation($"Admin_TpBetRecordRelayGroupByDate_Get ms:{sw.Elapsed.TotalMilliseconds}");
            return data;
        }

        public Task<IEnumerable<TResult>> GetMapTpBetRecordByPlayer<TResult>(int siteId, DateTimeOffset starTime, DateTimeOffset endTime, string format,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var closingBalanceTimeZone = siteId == 1 ? " 00:00:00" : " 23:00:00"; // +8時區 00:00:00  +7時區 23:00:00
            var closingEndTime = siteId == 1 ? endTime.ToUniversalTime().DateTime : endTime.ToUniversalTime().DateTime.AddDays(-1); // +8時區今天 +7 時區-1天

            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_DateType", (int)mpaDateType, dbType: DbType.Int32);
            p.Add("_ClosingEndTime", closingEndTime, dbType: DbType.DateTime);
            p.Add("_ClosingBalanceTimeZone", closingBalanceTimeZone, dbType: DbType.String);
            return _reportDbClient.QueryAsync<TResult>("Admin_TpBetRecordRelayGroupByPlayer_Get", p);
        }

        public Task<IEnumerable<TResult>> GetMapTransaction<TResult>(int siteId, DateTimeOffset starTime, DateTimeOffset endTime, string format)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);

            var sw = Stopwatch.StartNew();
            sw.Start();
            var data = _reportDbClient.QueryAsync<TResult>("Admin_TransactionRelayGroupByDate_Get", p);
            sw.Stop();
            _logger.LogInformation($"Admin_TransactionRelayGroupByDate_Get ms:{sw.Elapsed.TotalMilliseconds}");
            return data;
        }

        public Task<IEnumerable<TpGameTypeOptionDto>> GetTpGameTypeOption()
        {
            return _reportDbClient.QueryAsync<TpGameTypeOptionDto>("Common_TpGameType_Get");
        }

        public IEnumerable<Commission9WDto> Get9WCommission(DateTimeOffset starTime, DateTimeOffset endTime, string format,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var p = new DynamicParameters();
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_DateType", (int)mpaDateType, dbType: DbType.Int16);
            return _reportDbClient.Query<Commission9WDto>("Admin_9WCommission_Get", p);
        }

        public  Task<IEnumerable<TpCategoryDto>> GetTpCategory()
        {
            return _reportDbClient.QueryAsync<TpCategoryDto>("Admin_TPCategory_Get");
        }

        public Task<IEnumerable<MpaWinLossBonusDto>> GetMpaWinLossBonus(int siteId, DateTimeOffset starTime, DateTimeOffset endTime, string format)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);

            var sw = Stopwatch.StartNew();
            sw.Start();
            var data = _reportDbClient.QueryAsync<MpaWinLossBonusDto>("Admin_MpaWinLossBonus_Get", p);
            sw.Stop();
            _logger.LogInformation($"Admin_MpaWinLossBonus_Get ms:{sw.Elapsed.TotalMilliseconds}");
            return data;
        }
    }
}
