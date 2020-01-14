using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class AdjustmentService
    {
        private readonly IConfiguration configuration;
        private readonly Common common;
        private readonly WalletService walletService;

        public AdjustmentService(IConfiguration configuration, Common common, WalletService walletService)
        {
            this.configuration = configuration;
            this.common = common;
            this.walletService = walletService;
        }
        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            transId = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { adjustmentInfo.AdjustType, adjustmentInfo.CustId, adjustmentInfo.UserName, adjustmentInfo.CurrencyId, adjustmentInfo.SiteId, AdjustIdInfo = adjustmentInfo.AdjustIdInfo, adjustmentInfo.Amount, Prefix = $"A{common.GetEnv()}" });
            p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = conn.Execute("Adjustment_Ins", p, transaction: trans, commandType: CommandType.StoredProcedure);
            transId = p.Get<string>("TransId");
            if (result > 0)
            {
                List<int> forStatus = new List<int>();
                forStatus.Add((int)AdjustStatus.Pending);
                if (SetAdjustmentInfo(transId, adjustmentInfo.SiteId, $"Payment Adjustment: {adjustmentInfo.Amount * (adjustmentInfo.AdjustType == 1 ? 1 : -1)}", Operator, forStatus, AdjustStatus.Approved, adjustmentInfo, conn, trans) > 0)
                {
                    trans.Commit();
                    transId = p.Get<string>("TransId");
                }
                else
                {
                    result = 0;
                }
            }

            return result;
        }

        public int SetAdjustmentInfo(string transId, int siteId, string comment, string Operator, IEnumerable<int> forStatus, AdjustStatus status, AdjustmentInfo adjustmentInfo, MySqlConnection conn, MySqlTransaction trans)
        {
            int adjustId = 0;
            int type = 1 + adjustmentInfo.AdjustType;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out adjustId);

            int result = 0;
            try
            {
                string sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new { AdjustId = adjustId, SiteId = siteId, RefId = (string)null, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = conn.Execute("Adjustment_Set", p, trans, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    SetAdjustmentLog(adjustId, siteId, comment, (int)status, Operator);

                    if (status == AdjustStatus.Approved)
                    {
                        byte is1stDepoist = 0;
                        decimal afterAmount = 0;
                        decimal amount = adjustmentInfo.Amount;

                        if (adjustmentInfo.AdjustType == 2)
                        {
                            amount *= -1;
                        }
                        int custId = p.Get<int?>("CustId") ?? 0;
                        string UserName = p.Get<string>("UserName");
                        result = walletService.SetBalance(custId, siteId, UserName, amount, type, $"Payment Adjustment: {amount}", transId, out afterAmount, out is1stDepoist, conn, trans);

                        if (is1stDepoist == 1 && result > 0)
                        {
                            result = SetAdjustmentInfoFirstDeposit(adjustId, conn, trans);
                        }
                    }

                    if (result > 0)
                    {
                        //trans.Commit();
                        result = p.Get<int?>("CustId") ?? result;
                    }
                }
            }
            catch (Exception ex)
            {
                SetAdjustmentLog(adjustId, siteId, ex.Message, (int)AdjustStatus.Error, Operator);
            }

            return result;
        }
        private int SetAdjustmentInfoFirstDeposit(int adjustId, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("AdjustmentInfoFirstDeposit_Set", new { AdjustId = adjustId }, trans, commandType: CommandType.StoredProcedure);
        }
        private int SetAdjustmentLog(int adjustId, int siteId, string comment, int status, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("AdjustmentLog_Ins", new { AdjustId = adjustId, SiteId = siteId, Comment = comment, Action = status, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }
    }
}
