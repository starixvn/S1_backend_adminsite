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
    public class WithdrawService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;

        public WithdrawService(IConfiguration configuration, WalletService walletService)
        {
            this.configuration = configuration;
            this.walletService = walletService;
        }

        public IEnumerable<WithdrawRequest> GetWithdrawRequestList(int siteId)
        {

            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<WithdrawRequest>("WithdrawRequest_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
