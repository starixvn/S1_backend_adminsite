using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

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

        public IEnumerable<CustVerificationReward> GetCustVerificationReward(IEnumerable<int> siteIds, int currencyId, string userName,
            DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_Currency", currencyId);
            p.Add("_UserName", userName);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime.AddDays(1).AddSeconds(-1));
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<CustVerificationReward>("Admin_CustVerificationReward_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }
        
        public async Task<IEnumerable<CustVerificationReward>> GetCustVerificationRewardExport(IEnumerable<int> siteIds, int currencyId, string userName, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_Currency", currencyId);
            p.Add("_UserName", userName);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime.AddDays(1).AddSeconds(-1));

            var result = await _reportDbClient.QueryAsync<CustVerificationReward>("Admin_CustVerificationRewardExport_Get", p);
            return result;
        }
    }
}