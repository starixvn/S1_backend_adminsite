using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                    para.Add(new ReferralList
                    {
                        SiteId = x.SiteId,
                        CurrencyId = x.CurrencyId,
                        PeriodDays = int.Parse(x.PeriodDays),
                        BonusAmount = decimal.Parse(x.BonusAmount),
                        VaildBet = decimal.Parse(x.VaildBet),
                        Deposit = decimal.Parse(x.Deposit)
                    });
                }
            });
            _mainDbClient.Execute("Admin_Referral_Set", new DynamicParameters(para));
        }
        public void SetReferralStatus(int siteId, int currencyId, int status)
        {
            _mainDbClient.Execute("Admin_Referral_Status_Set_V2", new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId, _Status = status }));
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            string SelectSite = null;
            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            totalPages = 0;

            var p = new DynamicParameters(new { SelectSite, UserName = userName, CurrencyId = currencyId, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<ReferralCustList>("Admin_ReferralCustlist_Get", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }
        public IEnumerable<ReferralFriend> GetRefFriendList(int custId,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<ReferralFriend>("Admin_ReferralFriendList_Get_V3", props);
            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
