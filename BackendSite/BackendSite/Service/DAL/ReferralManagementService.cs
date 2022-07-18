using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class ReferralManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ReferralManagementService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<ReferralList> GetReferralBySiteId(int siteId)
        {
            return _mainDbClient.Query<ReferralList>("Admin_Referral_Get", new DynamicParameters(new { SiteId = siteId }));
        }
        
        public async Task<ReferralDetailData> GetReferralDetail(int siteId, int currencyId)
        {
            return await _reportDbClient.QueryFirstAsync<ReferralDetailData>("Admin_ReferralDetail_Get",
                new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId }));
        }
        
        public void SetReferral(List<ReferralManagement> referralList)
        {
            var para = new List<ReferralList>();
            referralList.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.PeriodDays) && !string.IsNullOrWhiteSpace(x.BonusAmount) && !string.IsNullOrWhiteSpace(x.ValidBet) && !string.IsNullOrWhiteSpace(x.Deposit))
                {
                    var param = new DynamicParameters(
                    new
                    {
                        SiteId = x.SiteId,
                        CurrencyId = x.CurrencyId,
                        PeriodDays = int.Parse(x.PeriodDays),
                        BonusAmount = decimal.Parse(x.BonusAmount),
                        ValidBet = decimal.Parse(x.ValidBet),
                        Deposit = decimal.Parse(x.Deposit)
                    });
                    _mainDbClient.Execute("Admin_Referral_Set", param);
                }
            });
        }
        public async Task<int> SetReferralAsync(SetReferralRequest setReferralRequest)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", setReferralRequest.SiteId);
            p.Add("_CurrencyId", setReferralRequest.CurrencyId);
            p.Add("_BonusAmount", setReferralRequest.BonusAmount);
            p.Add("_InvitationLimit", setReferralRequest.InvitationLimit);
            p.Add("_AgentValidBet", setReferralRequest.AgentValidBet);
            p.Add("_AgentDeposit", setReferralRequest.AgentDeposit);
            p.Add("_ReferralFreeCredit", setReferralRequest.ReferralFreeCredit);
            p.Add("_ValidBet", setReferralRequest.ValidBet);
            p.Add("_Deposit", setReferralRequest.Deposit);
            p.Add("_PeriodDays", setReferralRequest.PeriodDays);
            p.Add("_SpecificList", JsonConvert.SerializeObject(setReferralRequest.SpecificList));

            return await _mainDbClient.ExecuteAsync("Admin_Referral_Set_V2", p);
        }
        
        public int SetReferralStatus(int siteId, int currencyId, int status)
        {
            return _mainDbClient.Execute("Admin_Referral_Status_Set_V2", new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId, _Status = status }));
        }
        
        public async Task<int> SetReferralStatusAsync(int siteId, int currencyId, int status)
        {
            return await _mainDbClient.ExecuteAsync("Admin_Referral_Status_Set_V2", new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId, _Status = status }));
        }

        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            string SelectSite = null;
            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            totalPages = 0;

            var p = new DynamicParameters(new { SelectSite, UserName = userName, CurrencyId = currencyId, PageSize = Math.Abs(pageSize) });
            p.Add("StartDate", startDate);
            p.Add("EndDate", endDate);
            p.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            p.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<ReferralCustList>("Admin_ReferralCustlist_Get_V2", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }
        
        public IEnumerable<ReferralExcel> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate)
        {
            var p = new DynamicParameters(new { selectSite, UserName = userName, CurrencyId = currencyId, StartDate = startDate, EndDate = endDate });

            var result = _reportDbClient.Query<ReferralExcel>("Admin_ReferralCustlistExport_Get_V3", p);
            return result;
        }

        public IEnumerable<ReferralFriend> GetRefFriendList(int custId,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int startIndex,
            out int TotalPages,
            out int RowCount,
            bool byRewardTime)
        {
            var SpName = string.Empty;
            if (byRewardTime)
            {
                SpName = "Admin_ReferralFriendListByRewardTime_Get";
            }
            else
            {
                SpName = "Admin_ReferralFriendList_Get_V3";
            }
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<ReferralFriend>(SpName, props);
            TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
