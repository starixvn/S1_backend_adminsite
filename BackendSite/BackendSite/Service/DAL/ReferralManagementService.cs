using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class ReferralManagementService
    {
        private readonly IConfiguration configuration;

        public ReferralManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<ReferralList> GetReferralBySiteId(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            return conn.Query<ReferralList>("Admin_Referral_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
        }
        public void SetReferral(List<ReferralManagement> referralList)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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
            conn.Execute("Admin_Referral_Set", para, commandType: CommandType.StoredProcedure);
        }
        public void SetReferralStatus(int SiteId, int CurrencyId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_Referral_Status_Set", new { SiteId, CurrencyId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate , DateTime? endDate , int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
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
            p.Add("StartIndex",(Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),DbType.Int32,ParameterDirection.Input);
            p.Add("PageNumber", Math.Abs(pageNumber),DbType.Int32,ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<ReferralCustList>("Admin_ReferralCustlist_Get_V2", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }
        
        public IEnumerable<ReferralCustList> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate)
        {
            var p = new DynamicParameters(new { selectSite, UserName = userName, CurrencyId = currencyId ,StartDate = startDate, EndDate= endDate});

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<ReferralCustList>("Admin_ReferralCustlistExport_Get", p, commandType: CommandType.StoredProcedure);
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            if (byRewardTime)
            { 
                SpName = "Admin_ReferralFriendListByRewardTime_Get";
            }else
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

            var result = conn.Query<ReferralFriend>(SpName, props, commandType: CommandType.StoredProcedure);
            TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
