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
            return _mainDbClient.Query<ReferralList>("Admin_Referral_Get", new DynamicParameters(new {SiteId = siteId }));
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
        public void SetReferralStatus(int siteId, int currencyId)
        {
            _mainDbClient.Execute("Admin_Referral_Status_Set", new DynamicParameters(new {SiteId = siteId, CurrencyId = currencyId }));
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
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<ReferralCustList>("Admin_ReferralCustlist_Get", p, commandType: CommandType.StoredProcedure);
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
            out int TotalPages,
            out int RowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Admin_ReferralFriendList_Get_V3";
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<ReferralFriend>(SpName, props, commandType: CommandType.StoredProcedure);
            TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
