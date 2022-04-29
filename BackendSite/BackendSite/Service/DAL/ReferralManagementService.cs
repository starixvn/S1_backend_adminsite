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
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ReferralManagementService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<ReferralList> GetReferralBySiteId(int siteId)
        {
            return _mainDbClient.Query<ReferralList>("Admin_Referral_Get", new DynamicParameters(new { SiteId = siteId }));
        }
        public void SetReferral(List<ReferralManagement> referralList)
        {
            var para = new List<ReferralList>();
            referralList.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.PeriodDays) && !string.IsNullOrWhiteSpace(x.BonusAmount) && !string.IsNullOrWhiteSpace(x.VaildBet) && !string.IsNullOrWhiteSpace(x.Deposit))
                {
                    //原本的寫法改用新的連線方式會有問題，先用迴圈Set
                    //para.Add(new ReferralList
                    //{
                    //    SiteId = x.SiteId,
                    //    CurrencyId = x.CurrencyId,
                    //    PeriodDays = int.Parse(x.PeriodDays),
                    //    BonusAmount = decimal.Parse(x.BonusAmount),
                    //    VaildBet = decimal.Parse(x.VaildBet),
                    //    Deposit = decimal.Parse(x.Deposit)
                    //});
                    var param = new DynamicParameters(
                    new
                    {
                        SiteId = x.SiteId,
                        CurrencyId = x.CurrencyId,
                        PeriodDays = int.Parse(x.PeriodDays),
                        BonusAmount = decimal.Parse(x.BonusAmount),
                        VaildBet = decimal.Parse(x.VaildBet),
                        Deposit = decimal.Parse(x.Deposit)
                    });
                    _mainDbClient.Execute("Admin_Referral_Set", param);
                }
                //_mainDbClient.Execute("Admin_Referral_Set", new DynamicParameters(para));
            });
        }
        public async Task<int> SetReferralAsync(SetReferralRequest setReferralRequest)
        {
            var p = new DynamicParameters();
            p.Add("SiteId", setReferralRequest.SiteId);
            p.Add("CurrencyId", setReferralRequest.CurrencyId);
            p.Add("PeriodDays", setReferralRequest.PeriodDays);
            p.Add("BonusAmount", setReferralRequest.BonusAmount);
            p.Add("InvitationLimit", setReferralRequest.InvitationLimit);
            p.Add("ReferralFreeCredit", setReferralRequest.ReferralFreeCredit);
            p.Add("VaildBet", setReferralRequest.VaildBet);
            p.Add("Deposit", setReferralRequest.Deposit);

            return await _mainDbClient.ExecuteAsync("Admin_Referral_Set", p);
        }
        
        public int SetReferralStatus(int siteId, int currencyId, int status)
        {
            return _mainDbClient.Execute("Admin_Referral_Status_Set_V2", new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId, _Status = status }));
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

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<ReferralCustList>("Admin_ReferralCustlist_Get_V2", p, commandType: CommandType.StoredProcedure);
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
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
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
