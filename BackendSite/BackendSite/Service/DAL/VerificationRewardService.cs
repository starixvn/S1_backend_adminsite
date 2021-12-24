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
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public VerificationRewardService(IEnumerable<MySqlClient> dbClient)
        {
            var mySqlClients = dbClient.ToList();
            _mainDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }
        public async Task<VerificationRewardModel> GetVerificationReward(int siteId, int currencyId)
        {
            var p = new DynamicParameters(new {_SiteId = siteId, _CurrencyId = currencyId});
            return await _reportDbClient.QueryFirstAsync<VerificationRewardModel>("Admin_VerificationReward_Get", p);
        }

        public async Task<int> InsertOrUpdate(int siteId, int currencyId, int verificationType, decimal phoneReward, decimal emailReward, string updateOperator)
        {
            var p = new DynamicParameters(new {_SiteId = siteId, _CurrencyId = currencyId,
                _VerificationType = verificationType, _PhoneReward = phoneReward, 
                _EmailReward = emailReward, _Operator = updateOperator});
            return await _mainDbClient.ExecuteAsync("Admin_VerificationReward_Set", p);
        }
        
        public async Task<int> UpdateStatus(int siteId, int currencyId, int status, string updateOperator)
        {
            var p = new DynamicParameters(new {_SiteId = siteId, _CurrencyId = currencyId, 
                                                       _Status = status, _Operator = updateOperator});
            return await _mainDbClient.ExecuteAsync("Admin_VerificationRewardStatus_Set", p);
        }
    }
}