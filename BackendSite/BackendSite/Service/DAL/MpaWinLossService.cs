using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class MpaWinLossService : IMpaWinLossService
    {
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<MpaWinLossService> _logger;

        public MpaWinLossService(IEnumerable<MySqlClient> sqlClients, ILogger<MpaWinLossService> logger)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
            _logger = logger;
        }

        public Task<IEnumerable<TResult>> GetMapTpBetRecord<TResult>(IEnumerable<int> siteIds, DateTimeOffset starTime, DateTimeOffset endTime, string format,
            string memberTypeJson, string userNameJson,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds), dbType: DbType.String);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_MemberTypeJson", memberTypeJson, dbType: DbType.String);
            p.Add("_UserNames", userNameJson, dbType: DbType.String);

            var data = _reportDbClient.QueryAsync<TResult>("Admin_TpBetRecordRelayGroupByDate_Get_V6", p, 120);

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

        public Task<IEnumerable<TResult>> GetMapTransaction<TResult>(IEnumerable<int> siteIds, DateTimeOffset starTime, DateTimeOffset endTime, string format,
            string memberTypeJson, string userNameJson)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds), dbType: DbType.String);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_MemberTypeJson", memberTypeJson, dbType: DbType.String);
            p.Add("_UserNames", userNameJson, dbType: DbType.String);

            var data = _reportDbClient.QueryAsync<TResult>("Admin_TransactionRelayGroupByDate_Get_V5", p);
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

        public Task<IEnumerable<TpCategoryDto>> GetTpCategory()
        {
            return _reportDbClient.QueryAsync<TpCategoryDto>("Admin_TPCategory_Get");
        }

        public Task<IEnumerable<MpaWinLossBonusDto>> GetMpaWinLossBonus(IEnumerable<int> siteIds, DateTimeOffset starTime, DateTimeOffset endTime, string format, string memberTypeJson)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds), dbType: DbType.String);
            p.Add("_StartTime", starTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_MemberTypeJson", memberTypeJson, dbType: DbType.String);

            var data = _reportDbClient.QueryAsync<MpaWinLossBonusDto>("Admin_MpaWinLossBonus_Get_V4", p);

            return data;
        }
    }
}
