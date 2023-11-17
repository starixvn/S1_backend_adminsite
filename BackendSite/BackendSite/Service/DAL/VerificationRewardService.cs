using System.Data;
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
            var p = new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId });
            return await _reportDbClient.QueryFirstAsync<VerificationRewardModel>("Admin_VerificationReward_Get", p);
        }

        public async Task<IEnumerable<VerificationRewardModel>> GetVerificationRewardBySiteId(int siteId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId });
            return await _reportDbClient.QueryAsync<VerificationRewardModel>("Admin_VerificationReward_GetBySiteId", p);
        }
        
        public async Task<int> InsertOrUpdate(int siteId, int currencyId, SetVerificationRewardParams input, string updateOperator)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
                _CurrencyId = currencyId,
                _VerificationType = (int)input.VerificationType,
                _PhoneReward = input.PhoneReward,
                _EmailReward = input.EmailReward,
                _MinDeposit = input.MinDeposit,
                _Operator = updateOperator
            });

            return await _mainDbClient.ExecuteAsync("Admin_VerificationReward_Set_V2", p);
        }

        public async Task<int> UpdateStatus(int siteId, int currencyId, int status, string updateOperator)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
                _CurrencyId = currencyId,
                _Status = status,
                _Operator = updateOperator
            });
            return await _mainDbClient.ExecuteAsync("Admin_VerificationRewardStatus_Set", p);
        }
        
        public IEnumerable<CustVerificationReward> GetCustVerificationRewardV2(int? custId,string multiSiteRequest,
            DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_MultiSiteAndCuurency", multiSiteRequest);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime);
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<CustVerificationReward>("Admin_CustVerificationReward_Get_V3", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }
        
        public async Task<IEnumerable<CustVerificationReward>> GetCustVerificationRewardExportV2(int? custId, string multiSiteRequest, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_MultiSiteAndCuurency", multiSiteRequest);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime);

            var result = await _reportDbClient.QueryAsync<CustVerificationReward>("Admin_CustVerificationRewardExport_Get_V3", p);
            return result;
        }
        
        public int InsertQueueJob(int custId, QueueJob type, string data)
        {
            var p = new DynamicParameters(new
            {
                _CustId = custId,
                _Type = (int) type,
                _Data = data
            });
            return _mainDbClient.Execute("Service_QueueJob_Ins", p);
        }
    }
}
