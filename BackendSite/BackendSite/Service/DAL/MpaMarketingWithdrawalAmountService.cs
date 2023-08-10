using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using Dapper;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class MpaMarketingWithdrawalAmountService: IMpaMarketingWithdrawalAmountService
    {
        private readonly MySqlClient _reportDbClient;
        public MpaMarketingWithdrawalAmountService(IEnumerable<MySqlClient> sqlClients)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public Task<IEnumerable<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountSummaryAsync(int siteId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", startTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);

            return _reportDbClient.QueryAsync<MpaMarketingWithdrawalAmountSummaryDto>("Admin_MpaMarketingWithdrawalAmountSummary_Get", p);
        }

        public Task<IEnumerable<MpaMarketingWithdrawalAmountDetailDto>> GetWithdrawalAmountDetailAsync(string siteList, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteList);
            p.Add("_StartTime", startTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime.ToUniversalTime().DateTime, dbType: DbType.DateTime);

            return _reportDbClient.QueryAsync<MpaMarketingWithdrawalAmountDetailDto>("Admin_MpaMarketingWithdrawalAmountDetail_Get_V2", p);
        }
    }
}
