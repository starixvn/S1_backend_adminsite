using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaMarketingDepositAmountService : IMpaMarketingDepositAmountService
    {
        private readonly MySqlClient _reportDbClient;

        public MpaMarketingDepositAmountService(IEnumerable<MySqlClient> sqlClients)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<MpaMarketingDepositAmountSummaryDto>> GetDepositAmountSummaryAsync(string siteList,
            DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteList);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            var result = await _reportDbClient.QueryAsync<MpaMarketingDepositAmountSummaryDto>("Admin_MpaMarketingDepositAmountSummary_Get_V2", p);
            return result;
        }

        public Task<IEnumerable<MpaMarketingDepositSignUpSummaryDto>> GetDepositSignUpSummaryAsync(int siteId, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            return _reportDbClient.QueryAsync<MpaMarketingDepositSignUpSummaryDto>("Admin_MpaMarketingDepositSigUpSummary_Get", p);
        }

        public IEnumerable<MpaMarketingDepositSignUpDetailDto> GetDepositSignUpDetail(string siteList, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteList);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            return _reportDbClient.Query<MpaMarketingDepositSignUpDetailDto>("Admin_MpaMarketingDepositSignUpDetail_Get_V2", p);
        }
    }
}
