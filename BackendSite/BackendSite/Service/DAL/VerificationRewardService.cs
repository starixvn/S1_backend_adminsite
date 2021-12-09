using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class VerificationRewardService
    {
        private readonly MySqlClient _reportDbClient;

        public VerificationRewardService(IEnumerable<MySqlClient> reportDbClient)
        {
            _reportDbClient = reportDbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }
        public async Task<IEnumerable<VerificationRewardModel>> GetVerificationRewardSetting(int siteId)
        {
            var p = new DynamicParameters(new {_SiteId = siteId});
            var verificationRewards = await _reportDbClient.QueryAsync<VerificationRewardModel>("Admin_VerificationReward_Get", p);
            return verificationRewards;
        }
    }
}