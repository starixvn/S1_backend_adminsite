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
                return conn.Query<BankDetail>("BankAccountOffList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> AddBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankAccountOffList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> EditBankDetail(int siteId, BankDetail bankDetail)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankAccountOffList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<BankDetail> DisableBankDetail(int siteId, BankDetail bankDetail, int direction)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<BankDetail>("BankAccountOffList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}