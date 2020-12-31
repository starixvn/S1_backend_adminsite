using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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
            List<ReferralList> para = new List<ReferralList>();
            referralList.ForEach(x =>
            {
                if(!string.IsNullOrWhiteSpace(x.PeriodDays) && !string.IsNullOrWhiteSpace(x.BonusAmount) && !string.IsNullOrWhiteSpace(x.VaildBet) && !string.IsNullOrWhiteSpace(x.Deposit))
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
    }
}
