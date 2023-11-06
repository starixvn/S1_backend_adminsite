using System.Data;
using System.Text.RegularExpressions;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class AdjustmentService
    {
        private readonly Common common;
        private readonly WalletService walletService;
        private readonly WithdrawalLimitService withdrawalLimitService;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<AdjustmentService> _logger;

        public AdjustmentService(IServiceProvider provider)
        {
            common = provider.GetRequiredService<Common>();
            walletService = provider.GetRequiredService<WalletService>();
            withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _logger = provider.GetRequiredService<ILogger<AdjustmentService>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            transId = null;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { adjustmentInfo.AdjustType, adjustmentInfo.CustId, adjustmentInfo.UserName, adjustmentInfo.CurrencyId, adjustmentInfo.SiteId, AdjustIdInfo = adjustmentInfo.AdjustIdInfo, adjustmentInfo.Amount, Prefix = $"A{common.GetEnv()}" });
            p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = _mainDbClient.Execute("Admin_Adjustment_Ins", trans, p);
            transId = p.Get<string>("TransId");
            if (result > 0)
            {
                var forStatus = new List<int>();
                forStatus.Add((int)AdjustmentInfoStatusEnum.Pending);
                if (SetAdjustmentInfo(transId, adjustmentInfo.SiteId, $"Payment Adjustment: {adjustmentInfo.Amount * (adjustmentInfo.AdjustType == 1 ? 1 : -1)}", Operator, forStatus, AdjustmentInfoStatusEnum.Approved, adjustmentInfo, trans) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    transId = p.Get<string>("TransId");
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    result = 0;
                }
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }

            return result;
        }

        public int SetAdjustmentInfo(string transId, int siteId, string comment, string Operator, IEnumerable<int> forStatus, AdjustmentInfoStatusEnum status, AdjustmentInfo adjustmentInfo, MySqlTransaction trans)
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
                var p = new DynamicParameters(new { AdjustId = adjustId, SiteId = siteId, RefId = adjustmentInfo.RefId, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = _mainDbClient.Execute("Adjustment_Set", trans, p);

                if (result > 0)
                {
                    SetAdjustmentLog(adjustId, siteId, comment, (int)status, Operator);

                    if (status == AdjustmentInfoStatusEnum.Approved)
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
                        result = walletService.SetBalance(custId, siteId, UserName, amount, type, $"Payment Adjustment: {amount}", transId, out afterAmount, out is1stDepoist, trans);

                        if (is1stDepoist == 1 && result > 0)
                        {
                            result = SetAdjustmentInfoFirstDeposit(adjustId, trans);
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
                SetAdjustmentLog(adjustId, siteId, ex.Message, (int)AdjustmentInfoStatusEnum.Error, Operator);
            }

            return result;
        }
        private int SetAdjustmentInfoFirstDeposit(int adjustId, MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("AdjustId", adjustId);
            return _mainDbClient.Execute("AdjustmentInfoFirstDeposit_Set", trans, props);
        }
        private void SetAdjustmentLog(int adjustId, int siteId, string comment, int status, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("AdjustId", adjustId);
            props.Add("SiteId", siteId);
            props.Add("Comment", comment);
            props.Add("Action", status);
            props.Add("Operator", Operator);
            _mainDbClient.Execute("AdjustmentLog_Ins", props);
        }

        public IEnumerable<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(int CustId,
            int PageSize,
            int StartIndex,
            out int TotalPages,
            out int RowCount)
        {
            var props = new DynamicParameters();
            props.Add("CustId", CustId);
            props.Add("PageSize", PageSize);
            props.Add("StartIndex", StartIndex);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<AdjustWithdrawalLimit>("Admin_AdjustWithdrawalLimitInfo_Get_V2", props);
            TotalPages = props.Get<int?>("TotalPages") ?? 0;
            RowCount = props.Get<int?>("RowCount") ?? 0;

            return result;
        }

        public (int ResultCode, string TransId) SetAdjustWithdrawalLimitV2(SetAdjustWithdrawalLimitParams input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _AdjustType = (int)input.AdjustType,
                _ReasonId = input.ReasonId,
                _CustId = input.CustId,
                _UserName = input.UserName,
                _Amount = input.Amount,
                _Memo = input.Memo,
                _Prefix = $"L{common.GetEnv()}",
                _Operator = input.Operator
            });
            p.Add("_TransId", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _ = _mainDbClient.Execute("Admin_AdjustWithdrawalLimitInfo_Ins_V2", trans, p);

            var transId = p.Get<string>("_TransId") ?? string.Empty;
            var resultCode = p.Get<int>("_Result");

            if (resultCode != 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return (resultCode, string.Empty);
            }

            var amount = input.AdjustType == WithdrawalLimitAdjustmentAdjustTypeEnum.Debit ? input.Amount * -1 : input.Amount;
            var result = withdrawalLimitService.SetWithdrawalLimit(input.CustId, input.UserName, input.SiteId,
                amount, (int)input.ReasonId, input.Memo, transId, (int)WithdrawalLimitLogCreateFromEnum.LimitAdjustment, input.Operator, out var afterAmount, trans);

            if (result < 1)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return (-2, string.Empty);
            }

            p = new DynamicParameters();
            p.Add("_SiteId", input.SiteId);
            p.Add("_CurrencyId", input.CurrencyId);
            p.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.WithdrawalLimitAdjustment);
            p.Add("_ActionType", (int)ActionType.Create);
            p.Add("_RefTransId", transId);
            p.Add("_BeforeDetailInfo", null, DbType.String);
            p.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                input.UserName,
                input.Amount,
                AdjustType = input.AdjustType.ToString(),
                Reason = input.ReasonId.ToString(),
                input.Memo
            }), DbType.String);
            p.Add("_Operator", input.Operator);

            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, p);

            if (logResult < 1)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return (-3, string.Empty);
            }

            _mainDbClient.CommitAndDisposeConnection(conn, trans);

            return (resultCode, transId);
        }

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo,
            string UserName,
            int CurrencyId,
            int SiteId,
            string Operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("CustId", withdrawalLimitInfo.CustId);
            var walletInfo = _mainDbClient.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", props).FirstOrDefault();
            decimal deduction = 0;
            if (walletInfo != null)
            {
                var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                deduction = tmpDeduction < 0 ? 0 : tmpDeduction;
            }

            if (withdrawalLimitInfo.AdjustType == 2 && deduction < withdrawalLimitInfo.Amount)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
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
                });
                p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

                _mainDbClient.Execute("Admin_AdjustWithdrawalLimitInfo_Ins", trans, p);
                var transId = p.Get<string>("TransId");

                if (string.IsNullOrEmpty(transId))
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 22;
                }
                else
                {
                    var amount = withdrawalLimitInfo.AdjustType == 2 ? withdrawalLimitInfo.Amount * -1 : withdrawalLimitInfo.Amount;
                    withdrawalLimitService.SetWithdrawalLimit(withdrawalLimitInfo.CustId, UserName, SiteId, amount, (int)WithdrawalLimitAdjustmentReasonEnum.None, withdrawalLimitInfo.Memo, transId, (int)WithdrawalLimitLogCreateFromEnum.LimitAdjustment, Operator, out var AfterAmount, trans);

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

                    _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", trans, propsLog);
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return 0;
                }
            }
        }

        public WithdrawalLimitInfo GetWithdrawalLimitInfo(int CustId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", CustId);
            return _mainDbClient.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", props).FirstOrDefault();
        }

        public int SetAdjustmentAndWithdrawalLimit(AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit, string Operator, out string Adjustment_transId)
        {
            Adjustment_transId = null;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var adjustmentP = new DynamicParameters(new
            {
                adjustmentAndWithdrawalLimit.AdjustType,
                adjustmentAndWithdrawalLimit.CustId,
                adjustmentAndWithdrawalLimit.UserName,
                adjustmentAndWithdrawalLimit.CurrencyId,
                adjustmentAndWithdrawalLimit.SiteId,
                adjustmentAndWithdrawalLimit.AdjustIdInfo,
                adjustmentAndWithdrawalLimit.Amount,
                Prefix = $"A{common.GetEnv()}",
                adjustmentAndWithdrawalLimit.Memo,
                adjustmentAndWithdrawalLimit.Category,
                adjustmentAndWithdrawalLimit.MethodType,
                adjustmentAndWithdrawalLimit.RefId
            });
            adjustmentP.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = _mainDbClient.Execute("Admin_Adjustment_Ins_V3", trans, adjustmentP);
            Adjustment_transId = adjustmentP.Get<string>("TransId");
            if (result > 0)
            {

                var forStatus = new List<int> { (int)AdjustmentInfoStatusEnum.Pending };
                result = SetAdjustmentInfo(Adjustment_transId, adjustmentAndWithdrawalLimit.SiteId, $"Payment Adjustment: {adjustmentAndWithdrawalLimit.Amount * (adjustmentAndWithdrawalLimit.AdjustType % 2 == 1/*1, 3(Add)2, 4(Deduct)*/ ? 1 : -1)}", Operator, forStatus, AdjustmentInfoStatusEnum.Approved, adjustmentAndWithdrawalLimit, trans);

                if (result > 0)
                {
                    Adjustment_transId = adjustmentP.Get<string>("TransId");
                    var isAdd = adjustmentAndWithdrawalLimit.AdjustType % 2 == 1; //1, 3(Add)2, 4(Deduct)
                    var isBalance = adjustmentAndWithdrawalLimit.AdjustType is 1 or 2;

                    //調帳類型為Main Wallet的現金錢包且為Deposit(1)時變更存款單實際存款金額
                    if (adjustmentAndWithdrawalLimit.Category == 1 && adjustmentAndWithdrawalLimit.MethodType == 5 && isBalance)
                    {
                        //判斷是否為調帳對象的存款單號
                        var isCustDeposit = getAdjustmentInfo(adjustmentAndWithdrawalLimit.CustId).Any(x => x == adjustmentAndWithdrawalLimit.RefId);
                        if (isCustDeposit)
                        {
                            var adjustAmount = isAdd
                           ? adjustmentAndWithdrawalLimit.Amount
                           : adjustmentAndWithdrawalLimit.Amount * -1;

                            result = AdjustmentActualDeposit(adjustmentAndWithdrawalLimit, Adjustment_transId,
                                adjustAmount, trans);
                        }
                        else
                        {
                            result = -1;
                        }
                    }

                    if (result > 0
                        && isAdd
                        && adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 1
                        && adjustmentAndWithdrawalLimit.WithdrawalLimitAmount > 0)
                    {
                        var props = new DynamicParameters();
                        props.Add("CustId", adjustmentAndWithdrawalLimit.CustId);
                        var walletInfo = _mainDbClient.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get", props).FirstOrDefault();
                        decimal deduction = 0;
                        if (walletInfo != null)
                        {
                            var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                            deduction = tmpDeduction < 0 ? 0 : tmpDeduction;
                        }

                        if (adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 2 && deduction < adjustmentAndWithdrawalLimit.WithdrawalLimitAmount)
                        {
                            //return 21;
                            result = 0;
                        }
                        else
                        {
                            var p = new DynamicParameters(new
                            {
                                _SiteId = adjustmentAndWithdrawalLimit.SiteId,
                                _CurrencyId = adjustmentAndWithdrawalLimit.CurrencyId,
                                _AdjustType = adjustmentAndWithdrawalLimit.AdjustType,
                                _ReasonId = WithdrawalLimitAdjustmentReasonEnum.None,
                                _CustId = adjustmentAndWithdrawalLimit.CustId,
                                _UserName = adjustmentAndWithdrawalLimit.UserName,
                                _Amount = adjustmentAndWithdrawalLimit.Amount,
                                _Memo = adjustmentAndWithdrawalLimit.Memo,
                                _Prefix = $"L{common.GetEnv()}",
                                _Operator = adjustmentAndWithdrawalLimit.Operator
                            });
                            p.Add("_TransId", dbType: DbType.String, direction: ParameterDirection.Output);
                            p.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                            _ = _mainDbClient.Execute("Admin_AdjustWithdrawalLimitInfo_Ins_V2", trans, p);

                            var transId = p.Get<string>("_TransId") ?? string.Empty;
                            var resultCode = p.Get<int>("_Result");

                            if (string.IsNullOrEmpty(transId) || resultCode != 0)
                            {
                                //return 22;
                                result = 0;
                            }
                            else
                            {
                                var amount = adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 2 ? adjustmentAndWithdrawalLimit.WithdrawalLimitAmount * -1 : adjustmentAndWithdrawalLimit.WithdrawalLimitAmount;
                                result = withdrawalLimitService.SetWithdrawalLimit(adjustmentAndWithdrawalLimit.CustId, adjustmentAndWithdrawalLimit.UserName, adjustmentAndWithdrawalLimit.SiteId, amount, adjustmentAndWithdrawalLimit.Category, adjustmentAndWithdrawalLimit.Memo, transId, (int)WithdrawalLimitLogCreateFromEnum.BalanceAdjustment, Operator, out _, trans);
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
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                }

            }

            return result;
        }

        private IEnumerable<string> getAdjustmentInfo(int custId)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId);

            var Result = _reportDbClient.Query<string>("Admin_DepositInfoTransId_Get", props);
            return Result;
        }

        public int AdjustmentActualDeposit(AdjustmentAndWithdrawalLimit input, string transId, decimal adjustAmount, MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("_DepositId", input.DepositId);
            props.Add("_AdjustAmount", adjustAmount);
            props.Add("_BeforeAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            props.Add("_AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            props.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _ = _mainDbClient.Execute("Admin_AdjustmentActualDeposit_Set", trans, props);

            var beforeAmount = props.Get<decimal>("_BeforeAmount");
            var afterAmount = props.Get<decimal>("_AfterAmount");
            var resultCode = props.Get<int>("_Result");

            if (resultCode != (int)ErrorCodeEnum.Success)
            {
                _logger.LogError($"AdjustmentActualDeposit update ActualDeposit failed, resultCode: {resultCode}, depositId: {input.DepositId}");

                return resultCode;
            }

            props = new DynamicParameters();
            props.Add("_TransId", transId);
            props.Add("_DepositId", input.DepositId);
            props.Add("_AdjustType", (int)AdjustWalletTypeEnum.Main);
            props.Add("_BeforeAmount", beforeAmount);
            props.Add("_AfterAmount", afterAmount);

            if (_mainDbClient.Execute("Admin_AdjustmentDepositLog_Ins", trans, props) < 1)
            {
                _logger.LogError($"AdjustmentDepositLog insert failed, depositId: {input.DepositId}");

                return -1;
            }

            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.BalanceAdjustment);
            props.Add("_ActionType", (int)ActionType.Create);
            props.Add("_RefTransId", transId);
            props.Add("_BeforeDetailInfo", null, DbType.String);
            props.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                input.UserName,
                Category = "Deposit",
                input.Memo,
                DepositTransId = input.TransId,
                BeforeActualDeposit = beforeAmount,
                AfterActualDeposit = afterAmount
            }), DbType.String);
            props.Add("_Operator", input.Operator);

            return _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, props);
        }

        public IEnumerable<AdjustmentLimitData> GetAdjustmentData(string siteId, string currencyId,
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {

            var props = new DynamicParameters();

            props.Add("_SiteIds", siteId);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<AdjustmentLimitData>("Admin_AdjustmentLimitInfo_Get_V2", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool SetAdjustmentLimitInfo(ParamsWithLog<AdjustmentLimit> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();
            var props = new DynamicParameters();
            props.Add("SiteId", input.SiteId);
            props.Add("CurrencyId", input.CurrencyId);
            props.Add("AdjustmentMax", input.After.AdjustmentMax);
            props.Add("Operator", input.Operator);

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.AdjustmentLimitation);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);
            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;

            if (!logResult)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, tran);
                return false;
            }

            var result = _mainDbClient.Execute("Admin_AdjustmentLimitInfo_Set", tran, props) > 0;

            if (!result)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, tran);
                return false;
            }

            _mainDbClient.CommitAndDisposeConnection(conn, tran);

            return true;
        }
        public CheckBalanceAdjustData CheckBalanceAdjustBonusCode(string bonusCode, int promotionType, int siteId, int currencyId)
        {
            var props = new DynamicParameters();
            props.Add("BonusCode", bonusCode);
            props.Add("SiteId", siteId);
            props.Add("PromotionType", promotionType);
            props.Add("CurrencyId", currencyId);
            return _reportDbClient.Query<CheckBalanceAdjustData>("Admin_CheckBalanceAdjust_Get_V2", props).FirstOrDefault();
        }
        public IEnumerable<CheckBalanceAdjustData> CheckBalanceAdjustBonusCodeBatch(string bonusCode, int promotionType, int siteId)
        {
            var props = new DynamicParameters();
            props.Add("BonusCode", bonusCode);
            props.Add("SiteId", siteId);
            props.Add("PromotionType", promotionType);
            props.Add("CurrencyId", 0);
            return _reportDbClient.Query<CheckBalanceAdjustData>("Admin_CheckBalanceAdjust_Get_V2", props);
        }

        public AdjustmentDepositLog GetAdjustmentDepositLogByTransId(string transId)
        {
            var props = new DynamicParameters();
            props.Add("_TransId", transId);

            return _reportDbClient.Query<AdjustmentDepositLog>("Admin_AdjustmentDepositLog_GetByTransId", props)
                .FirstOrDefault();
        }

        public CheckPayoutBonusCodeData CheckPayoutBonusCode(string bonusCode, int siteId, int currencyId)
        {
            var props = new DynamicParameters(new
            {
                BonusCode = bonusCode,
                SiteId = siteId,
                CurrencyId = currencyId,
            });
            return _reportDbClient.QueryFirst<CheckPayoutBonusCodeData>("Admin_CheckPayoutBonusCode_Get", props);
        }
    }
}
