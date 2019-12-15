using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.BLL
{
    public class BankDetailService
    {
        private readonly IConfiguration configuration;

        public BankDetailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<BankDetail> GetBankDetailList(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankDetail_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("BankDetail_Ins", new { SiteId = siteId, SysId = bankDetail.SysId, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel  }, commandType: CommandType.StoredProcedure)>0;
            }
        }

        public bool EditBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("BankDetail_Set", new { SiteId = siteId, SysId = bankDetail.BankDid, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure)>0;
            }
        }

        public IEnumerable<BankDetail> BankList(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankInfoSysList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}