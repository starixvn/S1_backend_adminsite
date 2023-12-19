using System.Data;
using System.Text.RegularExpressions;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class DepositService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private readonly Common common;
        private readonly WithdrawalLimitService withdrawalLimitService;
        private readonly BankFinancialService _bankFinancialService;
        private int payTypePosition = 100;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<DepositService> _logger;
        private readonly MySqlClient _testReportDbClient;

        public DepositService(IServiceProvider provider)
        {
            configuration = provider.GetRequiredService<IConfiguration>();
            walletService = provider.GetRequiredService<WalletService>();
            common = provider.GetRequiredService<Common>();
            withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _bankFinancialService = provider.GetRequiredService<BankFinancialService>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _logger = provider.GetRequiredService<ILogger<DepositService>>();
            _testReportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.TestReport);
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(
            IEnumerable<int> selectSite,
            IEnumerable<int> depositType,
            IEnumerable<string> selectBankCode,
            IEnumerable<int> selectCalCode,
            DateTime startDate,
            DateTime endDate,
            DepositDateTypeEnum dateType,
            string DepositId,
            IEnumerable<string> userNames,
            string agent,
            IEnumerable<int> currencyId,
            IEnumerable<int> status,
            IEnumerable<int> levelStatus,
            int pageSize,
            decimal startAmount,
            decimal endAmount,
            IEnumerable<int> payIds,
            int pageNumber, out int totalPages, out int rowCount,
            ref List<TotalAmount> currencyAmountRecord)
        {
            var depositId = 0;

            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }

            var p = new DynamicParameters(new
            {
                SelectSite = JsonConvert.SerializeObject(selectSite),
                DepositType = JsonConvert.SerializeObject(depositType),
                SelectBankCode = selectBankCode.Any() ? JsonConvert.SerializeObject(selectBankCode) : null,
                SelectCalCode = selectCalCode.Any() ? JsonConvert.SerializeObject(selectCalCode) : null,
                StartDate = startDate,
                EndDate = endDate,
                DateType = (int)dateType == 0 ? (int)DepositDateTypeEnum.CreateTime : (int)dateType,
                DepositId = depositId,
                TransId = string.IsNullOrEmpty(DepositId) ? null : DepositId,
                UserNames = userNames.Any() ? JsonConvert.SerializeObject(userNames) : null,
                Agent = string.IsNullOrEmpty(agent) ? null : agent,
                CurrencyId = currencyId.Any() ? JsonConvert.SerializeObject(currencyId) : null,
                Status = JsonConvert.SerializeObject(status),
                VipStatus = levelStatus.Any() ? JsonConvert.SerializeObject(levelStatus) : null,
                StartAmount = startAmount,
                EndAmount = endAmount,
                PayIds = payIds.Any() ? JsonConvert.SerializeObject(payIds) : null,
            });

            // GetCurrencyTotalAmount
            //currencyAmountRecord = _testReportDbClient != null 
            //    ? _testReportDbClient.Query<TotalAmount>("Admin_DepositTotalAmount_Get_V9", p).ToList()
            //    : _reportDbClient.Query<TotalAmount>("Admin_DepositTotalAmount_Get_V9", p).ToList();
            currencyAmountRecord = _reportDbClient.Query<TotalAmount>("Admin_DepositTotalAmount_Get_V9", p).ToList();

            p.Add("PageSize", pageSize, DbType.Int32, ParameterDirection.Input);
            p.Add("StartIndex", Math.Abs(pageNumber - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            //var depositList = _testReportDbClient != null 
            //    ? _testReportDbClient.Query<DepositRequest>("Admin_DepositList_Get_V13", p)
            //    : _reportDbClient.Query<DepositRequest>("Admin_DepositList_Get_V13", p);
            var depositList = _reportDbClient.Query<DepositRequest>("Admin_DepositList_Get_V13", p);

            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId,
            string TransId,
            int? Status,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount,
            out decimal totalAmount,
            out decimal totalApproveAmount,
            out int totalApproveRow)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", CustId, dbType: DbType.Int32);
            p.Add("_TransId", TransId, dbType: DbType.String);
            p.Add("_Status", Status, dbType: DbType.Int32);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_TotalAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_TotalApproveAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_TotalApproveRow", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var depositList = _reportDbClient.Query<DepositRequest>("Admin_DepositInfoListByCust_Get_V6", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            totalAmount = p.Get<decimal?>("_TotalAmount") ?? 0;
            totalApproveAmount = p.Get<decimal?>("_TotalApproveAmount") ?? 0;
            totalApproveRow = p.Get<int?>("_TotalApproveRow") ?? 0;

            return depositList;
        }

        public DepositRequest GetDepositRequest(int depositId)
        {
            return _mainDbClient.QueryFirst<DepositRequest>("Admin_DepositRequestTransID_Get_V4", new DynamicParameters(new { DepositId = depositId }));
        }

        public IEnumerable<DepositComments> GetDepositRequestComment(int depositId, int siteId)
        {
            return _mainDbClient.Query<DepositComments>("Admin_DepositRequestComment_Get",
                new DynamicParameters(new { _DepositId = depositId, _SiteId = siteId }));
        }

        public string InsertDepositRequest(DepositDetailInfo deposit)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                deposit.CustId,
                deposit.UserName,
                deposit.CurrencyId,
                deposit.SiteId,
                deposit.DepositInfo,
                deposit.Amount,
                deposit.Comment,
                deposit.RefId,
                DepositTime = deposit.DepositTime.ToUnixTimeSeconds(),
                deposit.Operator,
                Prefix = $"D{common.GetEnv()}"
            });

            var DepositInfo = JsonConvert.DeserializeObject(deposit.DepositInfo);
            var para = new
            {
                deposit.CustId,
                deposit.UserName,
                deposit.CurrencyId,
                deposit.SiteId,
                DepositInfo,
                deposit.Amount,
                deposit.Comment,
                deposit.RefId,
                DepositTime = deposit.DepositTime.ToUnixTimeSeconds(),
                deposit.Operator,
                Prefix = $"D{common.GetEnv()}"
            };
            var propsLog = new DynamicParameters();
            propsLog.Add("Menu", "DepositSubmission");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(para));
            propsLog.Add("Operator", deposit.Operator);

            _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", trans, propsLog);

            p.Add("vTransId", dbType: DbType.String, direction: ParameterDirection.Output);


            var result = _mainDbClient.Execute("Admin_DepositInfo_Ins", trans, p);
            var transId = p.Get<string>("vTransId");
            if (result > 0)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }

            return transId;
        }

        public async Task<int> SetDepositReceiptPic(int siteId, int custId, int depositId, int receiptNo, string picFileName, string creator)
        {
            var result = 0;

            try
            {
                await using var conn = _mainDbClient.GetMySqlConnection();
                await conn.OpenAsync();
                await using var trans = await conn.BeginTransactionAsync();

                var props = new DynamicParameters();
                props.Add("CustId", custId);
                props.Add("DepositId", depositId);
                props.Add("ReceiptNo", receiptNo);
                props.Add("PicFileName", picFileName);
                //props.Add("DepositPic", depositPic);
                props.Add("Creator", creator);
                props.Add("CreateFrom", 2);
                result = await _mainDbClient.ExecuteAsync("Admin_DepositReceiptPic_Set_V3", trans, props);

                if (result > 0)
                {
                    result = SetDepositLog(depositId, siteId, $"Upload Receipt , picFileName:{picFileName}  ", (int)DepositStatusEnum.UploadReceipt, 0, creator);
                }

                if (result > 0)
                {
                    await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                }
                else
                {
                    await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatusEnum.Error, 0, creator);
            }

            return result;
        }

        public async Task<int> DelDepositReceiptPic(int siteId, int custId, int depositId, string picFileName, int receiptNo, string Operator)
        {
            var result = 0;

            try
            {
                await using var conn = _mainDbClient.GetMySqlConnection();
                await conn.OpenAsync();
                await using var trans = await conn.BeginTransactionAsync();

                var props = new DynamicParameters();
                props.Add("CustId", custId);
                props.Add("DepositId", depositId);
                props.Add("ReceiptNo", receiptNo);
                result = await _mainDbClient.ExecuteAsync("Admin_DepositReceiptPic_Del", trans, props);

                if (result > 0)
                {
                    result = SetDepositLog(depositId, siteId, $"Delete Receipt, picFileName:{picFileName}", (int)DepositStatusEnum.DeleteReceipt, 0, Operator);
                }

                if (result > 0)
                {
                    await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                }
                else
                {
                    await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatusEnum.Error, 0, Operator);
            }

            return result;
        }

        public int CheckDepositReceipt(int siteId, int depositId, bool status, string Operator)
        {
            var result = 0;

            try
            {
                using var conn = _mainDbClient.GetMySqlConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                var props = new DynamicParameters();
                props.Add("_DepositId", depositId);
                props.Add("_Status", status);
                props.Add("_Operator", Operator);
                result = _mainDbClient.Execute("Admin_DepositCheckReceipt_Set_V2", props);

                if (result > 0)
                {
                    if (status)
                    {
                        result = SetDepositLog(depositId, siteId, "Check DepositReceipt",
                            (int)DepositStatusEnum.CheckDepositReceipt, 0, Operator);
                    }
                    else
                    {
                        result = SetDepositLog(depositId, siteId, "UnCheck DepositReceipt",
                            (int)DepositStatusEnum.UnCheckDepositReceipt, 0, Operator);
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatusEnum.Error, 0, Operator);
            }

            return result;
        }

        public int SetDepositInfo(string transId, int siteId, decimal actualDeposit, string comment, string Operator, IEnumerable<int> forStatus, DepositStatusEnum status, DepositRequest depositRequest, DepositCalCodeSetRequest calCodeInfo, bool canReward = false, string depositInfo = null)
        {
            var type = 1;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);

            var result = 0;
            try
            {
                var beforeDepositData = GetDepositRequest(depositId);

                using var conn = _mainDbClient.GetMySqlConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();
                var sForStatus = JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new
                {
                    DepositId = depositId,
                    SiteId = siteId,
                    ActualDeposit = actualDeposit,
                    Comment = comment,
                    ForStatus = sForStatus,
                    Status = (int)status,
                    ThirdId = (string)null,
                    Operator = Operator
                });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = _mainDbClient.Execute("Admin_DepositInfo_Set_V2", trans, p);

                if (result > 0)
                {
                    if (status is (DepositStatusEnum.Draft or DepositStatusEnum.Approved or DepositStatusEnum.OnHold) && calCodeInfo != null)
                    {
                        var calCodeList = _bankFinancialService.GetTransferAccountList();
                        calCodeInfo.DisplayName = calCodeList.FirstOrDefault(x => x.Id == calCodeInfo.BankId)?.DisplayName;

                        if (status == DepositStatusEnum.Draft)
                        {
                            comment = $"Comment : {comment} , CalCode :[Before:{beforeDepositData.DisplayName} , After:{calCodeInfo.DisplayName}] , StatementTime : [Before:{beforeDepositData.StatementTime} , After:{calCodeInfo.StatementTime}] ]";
                        }
                        else
                        {
                            comment = $"Comment : {comment} , CalCode :[Before:{beforeDepositData.DisplayName}, After:{calCodeInfo.DisplayName}] ";
                        }
                    }

                    SetDepositLog(depositId, siteId, comment, (int)status, actualDeposit, Operator);

                    if (status == DepositStatusEnum.Approved)
                    {
                        if (depositRequest.DepositType is not (DepositTypeEnum.LocalBanking or DepositTypeEnum.CryptoWallet))
                        {
                            var pay = (IPay)depositRequest.oDepositInfo;
                            type = payTypePosition + pay.PayId;
                        }

                        var custId = p.Get<int?>("CustId") ?? 0;
                        var userName = p.Get<string>("UserName");
                        result = walletService.SetBalance(custId, siteId, userName, actualDeposit, type, $"Deposit Approved: {actualDeposit}", transId, out _, out var is1stDepoist, trans);

                        if (result >= 0)
                        {
                            result = withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, actualDeposit, (int)WithdrawalLimitAdjustmentReasonEnum.Deposit, $"WithdrawalLimit: {actualDeposit}", transId, (int)WithdrawalLimitLogCreateFromEnum.Deposit, Operator, out _, trans);
                        }

                        if (is1stDepoist == 1 && result > 0)
                        {
                            result = SetDepositInfoFirstDeposit(depositId, siteId, trans);
                        }

                        if (result > 0 && !string.IsNullOrEmpty(depositInfo))
                        {
                            result = UpdateDepositInfo(depositRequest.TransId, depositInfo, trans);
                            if (result < 1)
                            {
                                _logger.LogError($"UpdateDepositInfo Failed. TransId: {transId}, depositInfo: {depositInfo}");
                            }
                        }

                        if (result > 0 && canReward)
                        {
                            var resultCode = InsertDepositRewardPointQueue(transId, trans);
                            if (resultCode != (int)ErrorCodeEnum.Success)
                            {
                                _logger.LogError($"InsertDepositRewardPointQueue Failed. ResultCode({resultCode}), TransId: {transId}");
                            }
                            else
                            {
                                result = 1;
                            }
                        }
                    }

                    if (status is (DepositStatusEnum.Draft or DepositStatusEnum.Approved or DepositStatusEnum.OnHold) && calCodeInfo != null)
                    {
                        var p2 = new DynamicParameters(new
                        {
                            DepositId = depositId,
                            SiteId = siteId,
                            StatementTime = status == DepositStatusEnum.Draft ? calCodeInfo.StatementTime?.DateTime : null,
                            BankId = calCodeInfo.BankId,
                            Operator = Operator
                        });
                        result = _mainDbClient.Execute("Admin_DepositInfoDraft_Set", trans, p2);
                    }

                    if (result > 0)
                    {
                        _mainDbClient.CommitAndDisposeConnection(conn, trans);
                        result = p.Get<int?>("CustId") ?? result;
                    }
                    else
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatusEnum.Error, actualDeposit, Operator);
            }

            return result;
        }

        private int UpdateDepositInfo(string transId, string depositInfo, IDbTransaction trans)
        {
            return _mainDbClient.Execute("Service_DepositInfoDepositInfo_Set",
                trans,
                new
                {
                    _TransId = transId,
                    _DepositInfo = depositInfo
                });
        }

        private int SetDepositInfoFirstDeposit(int depositId, int siteId, IDbTransaction trans)
        {
            return _mainDbClient.Execute("DepositInfoFirstDeposit_Set",
                trans,
                new DynamicParameters(new
                {
                    DepositId = depositId,
                    SiteId = siteId
                }));
        }

        public int SetDepositLog(int depositId, int siteId, string comment, int status, decimal actualDeposit, string Operator)
        {
            return _mainDbClient.Execute("DepositLog_Ins",
                new DynamicParameters(new
                {
                    DepositId = depositId,
                    SiteId = siteId,
                    Comment = comment,
                    Action = status,
                    ActualDeposit = actualDeposit,
                    Operator
                }));
        }

        public IEnumerable<DepositReceipt> GetDepositReceipt(int depositId, int siteId)
        {
            return _reportDbClient.Query<DepositReceipt>("DepositReceipt_Get", new DynamicParameters(new { DepositId = depositId, SiteId = siteId }));
        }

        public DepositReceipt GetDepositReceiptByNo(int depositId, int receiptNo, int siteId)
        {
            return _reportDbClient.QueryFirst<DepositReceipt>("Admin_DepositReceiptByNo_Get", new DynamicParameters(new { DepositId = depositId, ReceiptNo = receiptNo, SiteId = siteId }));
        }

        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string selectBankCode, string selectCalCode, string depositType, DateTime startDate, DateTime endDate, DepositDateTypeEnum dateType, string DepositId, IEnumerable<string> userNames, string agent, string currencyId, string status, string levelStatus, decimal startAmount, decimal endAmount, string payIds)
        {
            var depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }

            var p = new DynamicParameters(new
            {
                SelectSite = selectSite,
                SelectBankCode = selectBankCode,
                SelectCalCode = selectCalCode,
                DepositType = depositType,
                StartDate = startDate,
                EndDate = endDate,
                DateType = (int)dateType == 0 ? (int)DepositDateTypeEnum.CreateTime : (int)dateType,
                DepositId = depositId,
                TransId = DepositId,
                UserNames = userNames.Any() ? JsonConvert.SerializeObject(userNames) : null,
                Agent = agent,
                CurrencyId = currencyId,
                Status = status,
                VipStatus = levelStatus,
                StartAmount = startAmount,
                EndAmount = endAmount,
                PayIds = payIds
            });
            var depositList = _reportDbClient.Query<DepositInfoForExport>("Admin_DepositListForExport_Get_V14", p);
            return depositList;
        }

        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType)
        {
            return _mainDbClient.Query<DepositLimitCurrencyInfo>("Admin_DepositCurrencyList_Get", new DynamicParameters(new { SiteId = siteId, DepositType = depositType }));
        }

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(string siteId, string depositType, string currencyId, int startIndex, int pageSize, out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();

            props.Add("_SiteIds", siteId);
            props.Add("_DepositTypes", depositType);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<DepositLimitInfo>("Admin_DepositLimitInfo_Get_V2", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int currencyId)
        {
            var props = new DynamicParameters();

            props.Add("_SiteId", siteId);
            props.Add("_CurrencyId", currencyId);
            var result = _reportDbClient.Query<DepositLimitInfo>("Admin_DepositLimitInfoReNew_Get", props);

            return result;
        }

        public bool AddDepositLimitInfo(ParamsWithLog<DepositLimitModel> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                input.SiteId,
                input.After.DepositType,
                input.CurrencyId,
                input.After.VipLevel,
                input.After.DepositMin,
                input.After.DepositMax
            };

            var result = _mainDbClient.Execute("Admin_DepositLimitInfo_Ins", tran, param) > 0;
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Create);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", null, DbType.String);
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public bool AddDepositLimitInfo(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _Infos = JsonConvert.SerializeObject(input.After)
            };

            var result = _mainDbClient.Execute("Admin_DepositLimitInfoReNew_Ins", tran, param) > 0;
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Create);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", null, DbType.String);
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public bool EditDepositLimitInfo(ParamsWithLog<DepositLimitModel> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                input.SiteId,
                input.After.DepositType,
                input.CurrencyId,
                input.After.VipLevel,
                input.After.DepositMin,
                input.After.DepositMax,
                input.After.DepositStatus
            };
            var result = _mainDbClient.Execute("Admin_DepositLimitInfo_Set", tran, param) > 0;
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public bool EditDepositLimitInfo(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _Infos = JsonConvert.SerializeObject(input.After)
            };
            var result = _mainDbClient.Execute("Admin_DepositLimitInfoReNew_Set", tran, param) > 0;

            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int vipLevel)
        {
            return _mainDbClient.QueryFirst<DepositLimitInfo>("Admin_DepositLimitInfoDetail_Get", new DynamicParameters(new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId, VipLevel = vipLevel }));
        }

        public bool DeleteDepositLimitInfo(ParamsWithLog<DepositLimitModel> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                input.SiteId,
                input.CurrencyId,
                input.Before.VipLevel,
                input.Before.DepositType
            };
            var result = _mainDbClient.Execute("Admin_DepositLimitInfo_Del", tran, param) > 0;
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Delete);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", null, DbType.String);
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public bool DeleteDepositLimitInfoV2(ParamsWithLog<DepositLimitModel> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                input.SiteId,
                input.CurrencyId,
                input.Before.VipLevel
            };

            var result = _mainDbClient.Execute("Admin_DepositLimitInfoReNew_Del", tran, param) > 0;
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DepositLimitation);
                logParam.Add("_ActionType", (int)ActionType.Delete);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", null, DbType.String);
                logParam.Add("_Operator", input.Operator);

                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }

        public IEnumerable<DepositTimeList> DepositTimeReportGet(string siteIds,
            string currencyIds,
            string statuses,
            DateTime startDate,
            DateTime endDate,
            string depositId,
            int custId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount,
            out List<TotalAmount> currencyTotalAmount)
        {
            var props = new DynamicParameters();
            int.TryParse(depositId, out var tempDepositId);

            props.Add("_SiteIds", siteIds);
            props.Add("_DepositId", tempDepositId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_Statuses", statuses);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_CustId", custId);


            #region totalSummary

            currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_DepositStatusTimeTotal_Get", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            #endregion

            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<DepositTimeList>("Admin_DepositStatusTime_Get_V5", props).ToList();

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;
            return result;
        }
        public List<DepositTimeList> GetExportData(string siteIds, string depositId, DateTime startDate, DateTime endDate, string statuses, string currencyIds, int custId)
        {
            int.TryParse(depositId, out var tempDepositId);

            var props = new DynamicParameters();
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_DepositId", tempDepositId);
            props.Add("_Statuses", statuses);
            props.Add("_CustId", custId);
            return _reportDbClient.Query<DepositTimeList>("Admin_DepositStatusTimeExport_Get_V6", props).ToList();

        }
        public bool SetDepositRequestAmount(ParamsWithLog<SetAmountRequest> input)
        {
            var m = Regex.Match(input.TransId, @"\d+");
            int.TryParse(m.Value, out var depositId);

            var props = new DynamicParameters();
            props.Add("_DepositId", depositId);
            props.Add("_Amount", input.After.Amount);
            props.Add("_Operator", input.Operator);

            return _mainDbClient.Execute("Admin_DepositRequestAmountV2_Set", props) > 0;
        }

        public IEnumerable<PaymentGateWayOptions> GetPaymentGateWayOptions()
        {
            return _mainDbClient.Query<PaymentGateWayOptions>("Admin_PaymentGateWayOptions_Get");
        }

        public int InsertDepositRewardPointQueue(string transId, IDbTransaction trans)
        {
            var para = new DynamicParameters();
            para.Add("_TransId", transId);
            para.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            _ = _mainDbClient.Execute("Service_DepositRewardPointQueue_Ins", trans, para);
            var resultCode = para.Get<int?>("_Result") ?? -9;

            return resultCode;
        }

        public bool SetDepositMessage(ParamsWithLog<DepositMessageRequest> input)
        {
            var props = new DynamicParameters();
            props.Add("_DepositId", input.After.DepositId);
            props.Add("_Status", (int)input.After.Status);

            return _mainDbClient.Execute("Admin_DepositMessage_Set", props) > 0;
        }

        public IEnumerable<DepositMessage> GetDepositMessage(IEnumerable<int> depositIds)
        {
            var props = new DynamicParameters();
            props.Add("_DepositIds", JsonConvert.SerializeObject(depositIds));

            return _reportDbClient.Query<DepositMessage>("Admin_DepositMessage_Get", props);
        }
    }
}
