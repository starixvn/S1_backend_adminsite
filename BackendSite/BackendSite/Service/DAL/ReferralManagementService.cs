using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
            conn.Execute("Admin_Referral_Status_Set", new { SiteId , CurrencyId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId,int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            string SelectSite = null;
            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            totalPages = 0;

            var p = new DynamicParameters(new { SelectSite, UserName = userName, CurrencyId = currencyId, PageSize = pageSize});
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<ReferralCustList>("Admin_ReferralCustlist_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }
        public IEnumerable<ReferralFriend> GetRefFriendList(int custId, long? StartDate, long? EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Admin_ReferralFriendList_Get";
            var props = new DynamicParameters(new { CustId = custId });
            props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", PageSize);
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);

            var result = conn.Query<ReferralFriend>(SpName, props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("PageNumber") ?? 0;
            TotalPages = props.Get<int?>("TotalPages") ?? 0;
            RowCount = props.Get<int?>("RowCount") ?? 0;

            return result;
        }
    }
}
