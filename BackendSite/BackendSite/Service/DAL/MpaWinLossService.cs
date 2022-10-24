using System.Data;
using System.Security.Policy;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaWinLossService: IMpaWinLossService
    {
        private readonly MySqlClient _reportDbClient;

        public MpaWinLossService(IEnumerable<MySqlClient> sqlClients)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public Task<IEnumerable<TResult>> GetMapTpBetRecordByDate<TResult>(int siteId, DateTime starTime, DateTime endTime, string format,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_DateType", (int)mpaDateType, dbType: DbType.Int32);
            return _reportDbClient.QueryAsync<TResult>("Admin_TpBetRecordRelayGroupByDate_Get", p);
        }

        public Task<IEnumerable<TResult>> GetMapTransactionByDate<TResult>(int siteId, DateTime starTime, DateTime endTime, string format)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", starTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime, dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            return _reportDbClient.QueryAsync<TResult>("Admin_TransactionRelayGroupByDate_Get", p);
        }

        public Task<IEnumerable<MpaUSdRateDto>> GetMpaUsdRate()
        {
            return _reportDbClient.QueryAsync<MpaUSdRateDto>("Admin_CurrencyUSDRate_Get");
        }

        public int GetCustIdByUserName(int siteId, string userName)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_UserName", userName, dbType: DbType.String);
            return _reportDbClient.QueryFirst<int>("Admin_CustIdByUserName_Get", p);
        }

        public Task<IEnumerable<TpGameTypeOptionDto>> GetTpGameTypeOption()
        {
            return _reportDbClient.QueryAsync<TpGameTypeOptionDto>("Common_TpGameType_Get");
        }

        public IEnumerable<Commission9WDto> Get9WCommission(DateTime starTime, DateTime endTime, string format,
            MpaDateType mpaDateType = MpaDateType.SettledDate)
        {
            var p = new DynamicParameters();
            p.Add("_StartTime", starTime.ToUniversalTime(), dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime(), dbType: DbType.DateTime);
            p.Add("_Format", format, dbType: DbType.String);
            p.Add("_DateType", (int)mpaDateType, dbType: DbType.Int16);
            return _reportDbClient.Query<Commission9WDto>("Admin_9WCommission_Get", p);
        }
    }
}
