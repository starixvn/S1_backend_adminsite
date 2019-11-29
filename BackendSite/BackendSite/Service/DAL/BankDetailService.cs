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
                return conn.Query<BankDetail>("BankDetailList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> AddBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankDetail_Ins", new { SiteId = siteId, SysId = bankDetail.BankDid, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel  }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> EditBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankDetail_Set", new { SiteId = siteId, SysId = bankDetail.BankDid, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> BankInfoStatus(int siteId, int bankId, int status)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankDetailStatus_Set", new { SiteId = siteId, BankId = bankId, Status = status }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}