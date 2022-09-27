using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
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
        private readonly ILogger<AdjustmentService> _logger;

        public AdjustmentService(IServiceProvider provider)
        {
            configuration = provider.GetRequiredService<IConfiguration>();
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { adjustmentInfo.AdjustType, adjustmentInfo.CustId, adjustmentInfo.UserName, adjustmentInfo.CurrencyId, adjustmentInfo.SiteId, AdjustIdInfo = adjustmentInfo.AdjustIdInfo, adjustmentInfo.Amount, Prefix = $"A{common.GetEnv()}" });
            p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = _mainDbClient.Execute("Admin_Adjustment_Ins", p, conn, trans);
            transId = p.Get<string>("TransId");
            if (result > 0)
            {
                var forStatus = new List<int>();
                forStatus.Add((int)AdjustmentInfoStatusEnum.Pending);
                if (SetAdjustmentInfo(transId, adjustmentInfo.SiteId, $"Payment Adjustment: {adjustmentInfo.Amount * (adjustmentInfo.AdjustType == 1 ? 1 : -1)}", Operator, forStatus, AdjustmentInfoStatusEnum.Approved, adjustmentInfo, conn, trans) > 0)
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

        public int SetAdjustmentInfo(string transId, int siteId, string comment, string Operator, IEnumerable<int> forStatus, AdjustmentInfoStatusEnum status, AdjustmentInfo adjustmentInfo, MySqlConnection conn, MySqlTransaction trans)
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
                result = _mainDbClient.Execute("Adjustment_Set", p, conn, trans);

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
                SetAdjustmentLog(adjustId, siteId, ex.Message, (int)AdjustmentInfoStatusEnum.Error, Operator);
            }

            return result;
        }
        private int SetAdjustmentInfoFirstDeposit(int adjustId, MySqlConnection conn, MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("AdjustId", adjustId);
            return _mainDbClient.Execute("AdjustmentInfoFirstDeposit_Set", props, conn, trans);
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

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo,
            string UserName,
            int CurrencyId,
            int SiteId,
            string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("CustId", withdrawalLimitInfo.CustId);
            var walletInfo = _mainDbClient.Query<WithdrawalLimitInfo>("Member_CustWithdrawalLimit_Get",props).FirstOrDefault();
            decimal deduction = 0;
            if (walletInfo != null)
            {
                var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                deduction = tmpDeduction < 0 ? 0 : tmpDeduction;
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
                });
                p.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

                _mainDbClient.Execute("Admin_AdjustWithdrawalLimitInfo_Ins", p, conn, trans);
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

                    _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
                    trans.Commit();
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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
                adjustmentAndWithdrawalLimit.Category
            });
            adjustmentP.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = _mainDbClient.Execute("Admin_Adjustment_Ins_V2", adjustmentP, conn, trans);
            Adjustment_transId = adjustmentP.Get<string>("TransId");
            if (result > 0)
            {

                var forStatus = new List<int> { (int)AdjustmentInfoStatusEnum.Pending };
                result = SetAdjustmentInfo(Adjustment_transId, adjustmentAndWithdrawalLimit.SiteId, $"Payment Adjustment: {adjustmentAndWithdrawalLimit.Amount * (adjustmentAndWithdrawalLimit.AdjustType % 2 == 1/*1, 3(Add)2, 4(Deduct)*/ ? 1 : -1)}", Operator, forStatus, AdjustmentInfoStatusEnum.Approved, adjustmentAndWithdrawalLimit, conn, trans);

                if (result > 0)
                {
                    Adjustment_transId = adjustmentP.Get<string>("TransId");
                    var isAdd = adjustmentAndWithdrawalLimit.AdjustType % 2 == 1; //1, 3(Add)2, 4(Deduct)
                    
                    // 調帳類型為 Deposit 時變更存款單實際存款金額
                    if (adjustmentAndWithdrawalLimit.Category == (int)AdjustmentCategoryEnum.Deposit)
                    {
                        var adjustAmount = isAdd
                            ? adjustmentAndWithdrawalLimit.Amount
                            : adjustmentAndWithdrawalLimit.Amount * -1;

                        result = AdjustmentActualDeposit(adjustmentAndWithdrawalLimit, Adjustment_transId,
                            adjustAmount, conn, trans);
                        
                        
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
                            var withdrawalLimitP = new DynamicParameters(new
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
                            withdrawalLimitP.Add("TransId", dbType: DbType.String, direction: ParameterDirection.Output);

                            _mainDbClient.Execute("Admin_AdjustWithdrawalLimitInfo_Ins", withdrawalLimitP, conn, trans);
                            var transId = withdrawalLimitP.Get<string>("TransId");

                            if (string.IsNullOrEmpty(transId))
                            {
                                //return 22;
                                result = 0;
                            }
                            else
                            {
                                var amount = adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType == 2 ? adjustmentAndWithdrawalLimit.WithdrawalLimitAmount * -1 : adjustmentAndWithdrawalLimit.WithdrawalLimitAmount;
                                result = withdrawalLimitService.SetWithdrawalLimit(adjustmentAndWithdrawalLimit.CustId, adjustmentAndWithdrawalLimit.UserName, adjustmentAndWithdrawalLimit.SiteId, amount, adjustmentAndWithdrawalLimit.Memo, transId, out _, conn, trans);
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
        
        public int AdjustmentActualDeposit(AdjustmentAndWithdrawalLimit input, string transId, decimal adjustAmount,
            MySqlConnection conn, MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("_DepositId", input.DepositId);
            props.Add("_AdjustAmount", adjustAmount);
            props.Add("_BeforeAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            props.Add("_AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            props.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _ = _mainDbClient.Execute("Admin_AdjustmentActualDeposit_Set", props, conn, trans);

            var beforeAmount = props.Get<decimal>("_BeforeAmount");
            var afterAmount = props.Get<decimal>("_AfterAmount");
            var resultCode = props.Get<int>("_Result");

            if (resultCode != (int) ErrorCodeEnum.Success)
            {
                _logger.LogError($"AdjustmentActualDeposit update ActualDeposit failed, resultCode: {resultCode}, depositId: {input.DepositId}");
                
                return resultCode;
            }
            
            props = new DynamicParameters();
            props.Add("_TransId", transId);
            props.Add("_DepositId", input.DepositId);
            props.Add("_AdjustType", (int) AdjustWalletTypeEnum.Main);
            props.Add("_BeforeAmount", beforeAmount);
            props.Add("_AfterAmount", afterAmount);

            if (_mainDbClient.Execute("Admin_AdjustmentDepositLog_Ins", props, conn, trans) < 1)
            {
                _logger.LogError($"AdjustmentDepositLog insert failed, depositId: {input.DepositId}");
                
                return -1;
            }
            
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BalanceAdjustment);
            props.Add("_ActionType", (int) ActionType.Create);
            props.Add("_RefTransId", transId);
            props.Add("_BeforeDetailInfo", null, DbType.String);
            props.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                input.UserName,
                Category = AdjustmentCategoryEnum.Deposit.ToString(),
                input.Memo,
                DepositTransId = input.TransId,
                BeforeActualDeposit = beforeAmount,
                AfterActualDeposit = afterAmount
            }), DbType.String);
            props.Add("_Operator", input.Operator);
            
            return _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", props, conn, trans);
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var props = new DynamicParameters();
            props.Add("SiteId", input.SiteId);
            props.Add("CurrencyId", input.CurrencyId);
            props.Add("AdjustmentMax", input.After.AdjustmentMax);
            props.Add("Operator", input.Operator);

            var logParam = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.AdjustmentLimitation);
            props.Add("_ActionType", (int)ActionType.Update);
            props.Add("_RefTransId", string.Empty);
            props.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            props.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            props.Add("_Operator", input.Operator);
            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, conn, tran) > 0;

            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = _mainDbClient.Execute("Admin_AdjustmentLimitInfo_Set", props, conn, tran) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

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
            var props = new DynamicParameters(new {
                BonusCode = bonusCode,
                SiteId = siteId,
                CurrencyId = currencyId,
            }); 
            return _reportDbClient.QueryFirst<CheckPayoutBonusCodeData>("Admin_CheckPayoutBonusCode_Get", props);
        }
    }
}
