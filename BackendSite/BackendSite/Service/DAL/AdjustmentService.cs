using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace BackendSite.Service.DAL
{
    public class AdjustmentService
    {
        private readonly IConfiguration configuration;
        private readonly Common common;
        private readonly WalletService walletService;
        private readonly WithdrawalLimitService withdrawalLimitService;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AdjustmentService(IConfiguration configuration, Common common, WalletService walletService, WithdrawalLimitService withdrawalLimitService, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            this.common = common;
            this.walletService = walletService;
            this.withdrawalLimitService = withdrawalLimitService;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }
        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            transId = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { adjustmentInfo.AdjustType, adjustmentInfo.CustId, adjustmentInfo.UserName, adjustmentInfo.CurrencyId, adjustmentInfo.SiteId, AdjustIdInfo = adjustmentInfo.AdjustIdInfo, adjustmentInfo.Amount, Prefix = $"A{common.GetEnv()}" });
            p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = conn.Execute("Admin_Adjustment_Ins", p, transaction: trans, commandType: CommandType.StoredProcedure);
            transId = p.Get<string>("TransId");
            if (result > 0)
            {
                var forStatus = new List<int>();
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
            var adjustId = 0;
            var type = 1 + adjustmentInfo.AdjustType;

            if (adjustmentInfo.AdjustType == 3)
            {
                type = 8;
            }
            else if (adjustmentInfo.AdjustType == 4)
            {
                type = 7;
            }

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out adjustId);

            var result = 0;
            try
            {
                var sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
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
                        var amount = adjustmentInfo.Amount;

                        if (adjustmentInfo.AdjustType == 2 || adjustmentInfo.AdjustType == 4)
                        {
                            amount *= -1;
                        }

                        var custId = p.Get<int?>("CustId") ?? 0;
                        var UserName = p.Get<string>("UserName");
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

        public IEnumerable<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<AdjustWithdrawalLimit> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_AdjustWithdrawalLimitInfo_Get";
            var props = new DynamicParameters();
            props.Add("@CustId", CustId);
            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", PageSize);

            result = conn.Query<AdjustWithdrawalLimit>(SpName, props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo, string UserName, int CurrencyId, int SiteId, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var walletInfo = conn.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", new { withdrawalLimitInfo.CustId }, trans, commandType: CommandType.StoredProcedure).FirstOrDefault();
            decimal deduction = 0;
            if (walletInfo != null)
            {
                var Balance = walletInfo.Balance;
                var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                deduction = tmpDeduction < 0 ? 0 : tmpDeduction;
                var tmpAvailableBalance = walletInfo.Balance - (deduction);
            }

            if (withdrawalLimitInfo.AdjustType == 2 && deduction < withdrawalLimitInfo.Amount)
            {
                trans.Commit();
                return 21;
            }
            else
            {
                var p = new DynamicParameters(new
                {
                    AdjustType = withdrawalLimitInfo.AdjustType,
                    CustId = withdrawalLimitInfo.CustId,
                    UserName = UserName,
                    Operator = withdrawalLimitInfo.Operator,
                    CurrencyId = CurrencyId,
                    SiteId = SiteId,
                    Amount = withdrawalLimitInfo.Amount,
                    Memo = withdrawalLimitInfo.Memo,
                    Prefix = $"L{common.GetEnv()}"
                }); ;
                p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

                conn.Execute("Admin_AdjustWithdrawalLimitInfo_Ins", p, trans, commandType: CommandType.StoredProcedure);
                var transId = p.Get<string>("TransId");

                if (string.IsNullOrEmpty(transId))
                {
                    trans.Rollback();
                    return 22;
                }
                else
                {
                    var amount = withdrawalLimitInfo.AdjustType == 2 ? withdrawalLimitInfo.Amount * -1 : withdrawalLimitInfo.Amount;
                    withdrawalLimitService.SetWithdrawalLimit(withdrawalLimitInfo.CustId, UserName, SiteId, amount, withdrawalLimitInfo.Memo, transId, out var AfterAmount, conn, trans);

                    #region propsLog
                    var propsLog = new DynamicParameters();
                    var list = new Dictionary<string, string>();
                    foreach (var paraName in p.ParameterNames)
                    {
                        var value = Convert.ToString(p.Get<dynamic>(paraName));
                        list.Add(paraName, value);
                    }
                    propsLog.Add("Menu", "WithdrawalLimitAdjustment");
                    propsLog.Add("ActionType", 1);
                    propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
                    propsLog.Add("Operator", Operator);
                    #endregion

                    conn.Execute("Admin_AdjustmentInfoLog_Set", propsLog, trans, commandType: CommandType.StoredProcedure);
                    trans.Commit();
                    return 0;
                }
            }
        }

        public WithdrawalLimitInfo GetWithdrawalLimitInfo(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", new { CustId }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public int SetAdjustmentAndWithdrawalLimit(AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit, string Operator, out string Adjustment_transId)
        {
            Adjustment_transId = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Adjustment_p = new DynamicParameters(new { adjustmentAndWithdrawalLimit.AdjustType, adjustmentAndWithdrawalLimit.CustId, adjustmentAndWithdrawalLimit.UserName, adjustmentAndWithdrawalLimit.CurrencyId, adjustmentAndWithdrawalLimit.SiteId, adjustmentAndWithdrawalLimit.AdjustIdInfo, adjustmentAndWithdrawalLimit.Amount, Prefix = $"A{common.GetEnv()}", adjustmentAndWithdrawalLimit.Memo });
            Adjustment_p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = conn.Execute("Admin_Adjustment_Ins", Adjustment_p, transaction: trans, commandType: CommandType.StoredProcedure);
            Adjustment_transId = Adjustment_p.Get<string>("TransId");
            if (result > 0)
            {
                var forStatus = new List<int>();
                forStatus.Add((int)AdjustStatus.Pending);
                result = SetAdjustmentInfo(Adjustment_transId, adjustmentAndWithdrawalLimit.SiteId, $"Payment Adjustment: {adjustmentAndWithdrawalLimit.Amount * (adjustmentAndWithdrawalLimit.AdjustType % 2 == 1/*1, 3(Add)2, 4(Deduct)*/ ? 1 : -1)}", Operator, forStatus, AdjustStatus.Approved, adjustmentAndWithdrawalLimit, conn, trans);

                if (result > 0)
                {
                    Adjustment_transId = Adjustment_p.Get<string>("TransId");

                    if (adjustmentAndWithdrawalLimit.AdjustType % 2 == 1/*1, 3(Add)2, 4(Deduct)*/  && adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 1 && adjustmentAndWithdrawalLimit.WithdrawalLimitAmount > 0)
                    {
                        var walletInfo = conn.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", new { adjustmentAndWithdrawalLimit.CustId }, trans, commandType: CommandType.StoredProcedure).FirstOrDefault();
                        decimal deduction = 0;
                        if (walletInfo != null)
                        {
                            var Balance = walletInfo.Balance;
                            var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                            deduction = tmpDeduction < 0 ? 0 : tmpDeduction;
                            var tmpAvailableBalance = walletInfo.Balance - (deduction);
                        }

                        if (adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 2 && deduction < adjustmentAndWithdrawalLimit.WithdrawalLimitAmount)
                        {
                            //return 21;
                            result = 0;
                        }
                        else
                        {
                            var WithdrawalLimit_p = new DynamicParameters(new
                            {
                                AdjustType = adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType,
                                CustId = adjustmentAndWithdrawalLimit.CustId,
                                UserName = adjustmentAndWithdrawalLimit.UserName,
                                Operator = adjustmentAndWithdrawalLimit.Operator,
                                CurrencyId = adjustmentAndWithdrawalLimit.CurrencyId,
                                SiteId = adjustmentAndWithdrawalLimit.SiteId,
                                Amount = adjustmentAndWithdrawalLimit.WithdrawalLimitAmount,
                                Memo = adjustmentAndWithdrawalLimit.Memo,
                                Prefix = $"L{common.GetEnv()}"
                            });
                            WithdrawalLimit_p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

                            conn.Execute("Admin_AdjustWithdrawalLimitInfo_Ins", WithdrawalLimit_p, trans, commandType: CommandType.StoredProcedure);
                            var transId = WithdrawalLimit_p.Get<string>("TransId");

                            if (string.IsNullOrEmpty(transId))
                            {
                                //return 22;
                                result = 0;
                            }
                            else
                            {
                                var amount = adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 2 ? adjustmentAndWithdrawalLimit.WithdrawalLimitAmount * -1 : adjustmentAndWithdrawalLimit.WithdrawalLimitAmount;
                                result = withdrawalLimitService.SetWithdrawalLimit(adjustmentAndWithdrawalLimit.CustId, adjustmentAndWithdrawalLimit.UserName, adjustmentAndWithdrawalLimit.SiteId, amount, adjustmentAndWithdrawalLimit.Memo, transId, out var AfterAmount, conn, trans);

                                //return 0;
                            }
                        }
                    }
                }
                else
                {
                    result = 0;
                }

                if (result > 0)
                {
                    trans.Commit();
                }
                else
                {
                    trans.Rollback();
                }

            }
            return result;
        }

        public AdjustmentLimtData GetAdjustmentData(int SiteId, int CurrencyId)
        {
            return _reportDbClient.Query<AdjustmentLimtData>("Admin_AdjustmentLimitInfo_Get", new DynamicParameters(new { SiteId = SiteId, CurrencyId= CurrencyId })).FirstOrDefault();
        }
        public int AddAdjustmentLimitInfo(int SiteId, int CurrencyId, double AdjustmentMax, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", SiteId);
            props.Add("CurrencyId", CurrencyId);
            props.Add("AdjustmentMax", AdjustmentMax);
            props.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_AdjustmentLimitInfo_Set", props);
        }
        public CheckBalanceAdjustData CheckBalanceAdjustBonusCode(string BonusCode, int PromotionType, int SiteId,int CurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("BonusCode", BonusCode);
            props.Add("SiteId", SiteId);
            props.Add("PromotionType", PromotionType);
            props.Add("CurrencyId", CurrencyId);
            return _reportDbClient.Query<CheckBalanceAdjustData>("Admin_CheckBalanceAdjust_Get", props).FirstOrDefault();
        }
    }
}
