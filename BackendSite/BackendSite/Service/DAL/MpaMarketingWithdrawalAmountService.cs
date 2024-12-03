using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaMarketingWithdrawalAmountService : IMpaMarketingWithdrawalAmountService
    {
        private readonly MySqlClient _reportDbClient;
        public MpaMarketingWithdrawalAmountService(IEnumerable<MySqlClient> sqlClients)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountSummaryAsync(string siteList, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteList);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            return await _reportDbClient.QueryAsync<MpaMarketingWithdrawalAmountSummaryDto>("Admin_MpaMarketingWithdrawalAmountSummary_Get_V2", p);
        }

        public async Task<IEnumerable<MpaMarketingWithdrawalAmountDetailDto>> GetWithdrawalAmountDetailAsync(string siteList, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteList);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            return await _reportDbClient.QueryAsync<MpaMarketingWithdrawalAmountDetailDto>("Admin_MpaMarketingWithdrawalAmountDetail_Get_V2", p);
        }
    }
}
