using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

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
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfo_Get", new { SiteId = siteId, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string UpdateOperator)
        {
            var result = false;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
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
                    conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

                    result = conn.Execute("Admin_WithdrawalLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
                    tran.Commit();
                }
            }
            return result;
        }

        public bool EditWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage,string UpdateOperator)
        {
            var result = false;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
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

                    conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator= UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

                    result = conn.Execute("Admin_WithdrawalLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
                    tran.Commit();
                }
                   
            }
            return result;
        }

        public WithdrawalLimitInfoManage GetWithdrawalLimitInfo(int siteId, int currencyId, int viplevel)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
            }
        }

        public int DeleteWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string Operator)
        {
            var result = 0;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var param = new
                    {
                        withdrawalLimitInfoManage.SiteId,
                        withdrawalLimitInfoManage.CurrencyId,
                        withdrawalLimitInfoManage.VipLevel
                    };
                    conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator = Operator }, tran, commandType: CommandType.StoredProcedure);

                    result = conn.Execute("Admin_WithdrawalLimitInfo_Del", new { SiteId = withdrawalLimitInfoManage.SiteId, CurrencyId = withdrawalLimitInfoManage.CurrencyId, VipLevel = withdrawalLimitInfoManage.VipLevel }, commandType: CommandType.StoredProcedure);
                    tran.Commit();
                }        
            }
            return result;
        }
    }
}
