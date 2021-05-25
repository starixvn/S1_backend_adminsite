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
    public class WithdrawalLimitService
    {
        private readonly IConfiguration configuration;

        public WithdrawalLimitService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public int SetWithdrawalLimit(int custId, string userName, int siteId, decimal withdrawalLimit, string memo, string refTransId, out decimal afterWithdrawalLimit, MySqlConnection conn, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, UserName = userName, SiteId = siteId, WithdrawalLimit = withdrawalLimit, Memo = memo, RefTransId = refTransId });
            p.Add("AfterWithdrawalLimit", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = conn.Execute("Common_WithdrawalLimit_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
            afterWithdrawalLimit = p.Get<decimal?>("AfterWithdrawalLimit") ?? 0;
            return result;
        }
        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfo_Get", new { SiteId = siteId, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string UpdateOperator)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    withdrawalLimitInfoManage.SiteId,
                    withdrawalLimitInfoManage.CurrencyId,
                    withdrawalLimitInfoManage.VipLevel,
                    withdrawalLimitInfoManage.WithdrawalMin,
                    withdrawalLimitInfoManage.WithdrawalMax,
                    UpdateOperator
                };
                return conn.Execute("Admin_WithdrawalLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public bool EditWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage,string UpdateOperator)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    withdrawalLimitInfoManage.SiteId,
                    withdrawalLimitInfoManage.CurrencyId,
                    withdrawalLimitInfoManage.VipLevel,
                    withdrawalLimitInfoManage.WithdrawalMin,
                    withdrawalLimitInfoManage.WithdrawalMax,
                    UpdateOperator
                };
                return conn.Execute("Admin_WithdrawalLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public WithdrawalLimitInfoManage GetWithdrawalLimitInfo(int siteId, int currencyId, int viplevel)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
            }
        }

        public int DeleteWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_WithdrawalLimitInfo_Del", new { SiteId = withdrawalLimitInfoManage.SiteId, CurrencyId = withdrawalLimitInfoManage.CurrencyId, VipLevel = withdrawalLimitInfoManage.VipLevel }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
