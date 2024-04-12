using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;
using static BackendSite.Controllers.ApiV2.Finance.Adjustment.BalanceAdjustmentController;

namespace BackendSite.Service.DAL
{
    public class BankFinancialService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _reportbackupDbClient;

        public BankFinancialService(IServiceProvider provider)
        {            
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _reportbackupDbClient = provider.GetServices<MySqlClient>().FirstOrDefault(p => p.DataBase == DataBase.ReportBackupDB);
        }

        #region Library Methods

        public IEnumerable<BankFinancialReasonModel> GetReason(string funcKey)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _FuncKey = funcKey
                });

            var SpName = "Admin_BankFinancialReason_GetByFuncKey";
            return _reportDbClient.Query<BankFinancialReasonModel>(SpName, parameters);
        }
        public IEnumerable<BankFinancialReasonModel> GetReasonByAdjustmentType(AdjustmentType adjustmentType)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _MethodType = (int)adjustmentType.MethodTypeID,
                    _AdjustType = (int)adjustmentType.AdjustmentTypeID
                });

            var SpName = "Admin_ReasonGroup_Get";
            var funcKey = _reportDbClient.Query<string>(SpName, parameters).FirstOrDefault();
            return GetReason(funcKey);
        }
         
        public (string, string) NoAssign(string prefix)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_Prefix", prefix);
            parameters.Add("_NowDate", direction: ParameterDirection.Output);
            parameters.Add("_NowMaxNo", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var SpName = "Admin_BankFinancialNoAssignForGetNo_Upd";
            _mainDbClient.Execute(SpName, parameters);

            return ((parameters.Get<string>("_NowDate") ?? string.Empty), (parameters.Get<int?>("_NowMaxNo") ?? 0).ToString());
        }

        #endregion

        #region Transfer Account

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoListV2(GetTransferAccountInfoListParams input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _IsSetting = input.IsSetting,
                    _SiteId = input.SiteId,
                    _CurrencyIds = input.StrCurrencyIds,
                    _Channels = input.StrChannels,
                    _Agent = input.Agent,
                    _Status = input.StrStatus,
                    _Purposes = input.StrPurposes,
                    _SysIds = input.StrSysIds,
                    _AccountNo = input.AccountNo,
                    _Id = input.Id
                });

            return _reportbackupDbClient != null
                    ? _reportbackupDbClient.RetryQuery<BankFinancialTransferAccountDetailModel>("Admin_BankFinancialTransferAccount_Get_V4", parameters)
                    : _reportDbClient.Query<BankFinancialTransferAccountDetailModel>("Admin_BankFinancialTransferAccount_Get_V4", parameters);
        }

        public IEnumerable<string> GetTransferAccountAgentList(int siteId)
        {
            var p = new DynamicParameters(
                new
                {
                    _SiteId = siteId
                });

            return _reportDbClient.Query<string>("Admin_BankFinancialTransferAccountAgent_Get", p);
        }

        public bool CheckTransferAccountExists(int siteId, string accountNo)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = siteId,
                    _AccountNo = accountNo,
                });

            var result = _mainDbClient.Query<int>("Admin_BankFinancialTransferAccount_Check", parameters);

            return result.Any();
        }

        public int InsertTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = bankFinancialTransferAccount.SiteId,
                    _SiteName = bankFinancialTransferAccount.SiteName,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = JsonConvert.SerializeObject(bankFinancialTransferAccount.Purpose),
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _CryptoCurrencyId = bankFinancialTransferAccount.CryptoCurrencyId,
                    _Network = bankFinancialTransferAccount.Network?.ToString(),
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _AccountCode = bankFinancialTransferAccount.AccountCode,
                    _BankLoginUrl = bankFinancialTransferAccount.BankLoginUrl,
                    _BankLoginAccount = bankFinancialTransferAccount.BankLoginAccount,
                    _BankLoginPassword = bankFinancialTransferAccount.BankLoginPassword,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _FrontendDisplayName = bankFinancialTransferAccount.FrontendDisplayName,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _InitialTime = bankFinancialTransferAccount.InitialTime.UtcDateTime,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _TransferPin = bankFinancialTransferAccount.TransferPin,
                    _LoginPin = bankFinancialTransferAccount.LoginPin,
                    _MobilePin = bankFinancialTransferAccount.MobilePin,
                    _CorporateID = bankFinancialTransferAccount.CorporateID,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _Creator = bankFinancialTransferAccount.Creator,
                    _Updator = bankFinancialTransferAccount.Updator
                });

            return _mainDbClient.Execute("Admin_BankFinancialTransferAccount_Ins_V8", parameters);
        }

        public int UpdateTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _Id = bankFinancialTransferAccount.Id,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = JsonConvert.SerializeObject(bankFinancialTransferAccount.Purpose),
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _CryptoCurrencyId = bankFinancialTransferAccount.CryptoCurrencyId,
                    _Network = bankFinancialTransferAccount.Network?.ToString(),
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _AccountCode = bankFinancialTransferAccount.AccountCode,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _FrontendDisplayName = bankFinancialTransferAccount.FrontendDisplayName,
                    _BankLoginUrl = bankFinancialTransferAccount.BankLoginUrl,
                    _BankLoginAccount = bankFinancialTransferAccount.BankLoginAccount,
                    _BankLoginPassword = bankFinancialTransferAccount.BankLoginPassword,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _InitialTime = bankFinancialTransferAccount.InitialTime.UtcDateTime,
                    _TransferPin = bankFinancialTransferAccount.TransferPin,
                    _LoginPin = bankFinancialTransferAccount.LoginPin,
                    _MobilePin = bankFinancialTransferAccount.MobilePin,
                    _CorporateID = bankFinancialTransferAccount.CorporateID,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _Updator = bankFinancialTransferAccount.Updator
                });

            var SpName = "Admin_BankFinancialTransferAccount_Upd_V9";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public int InsertTransferAccountLog(ActionType action, BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = bankFinancialTransferAccount.SiteId,
                    _SiteName = bankFinancialTransferAccount.SiteName,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = bankFinancialTransferAccount.Purpose,
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _Operator = bankFinancialTransferAccount.Updator,
                    _Action = action
                });

            var SpName = "Admin_BankFinancialTransferAccountLog_Ins_V2";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountList()
        {
            var SpName = "Admin_BankFinancialTransferAccount_GetForList";
            return _reportDbClient.Query<BankFinancialTransferAccountDetailModel>(SpName);
        }

        public IEnumerable<BankFinancialTransferAccount> GetTransferAccountList(int siteId, IEnumerable<TransferAccountPurposeEnum> purposes)
        {
            var parameters = new DynamicParameters(new
            {
                _SiteId = siteId,
                _Purposes = purposes == null ? null : JsonConvert.SerializeObject(purposes)
            });
            return _reportDbClient
                .Query<BankFinancialTransferAccount>("Admin_BankFinancialTransferAccount_GetForList_V4", parameters);
        }

        public int GetTransferDataCount(string accountNo)
        {
            var count = 0;

            var parameters = new DynamicParameters(
                new
                {
                    _AccountNo = accountNo,
                });
            parameters.Add("_Count", count, DbType.Int32, ParameterDirection.InputOutput);

            var SpName = "Admin_BankFinancialTransferInfo_ByAccountNo_Get";
            _mainDbClient.Execute(SpName, parameters);

            count = parameters.Get<int?>("_Count") ?? 0;

            return count;
        }

        public int UpdateTransferInfo(BankFinancialTransferAccountModel bankFinancialTransferAccount, BankFinancialTransferAccountDetailModel oldData)
        {
            var affectCount = 0;
            var parameters = new DynamicParameters(
                new
                {
                    _OldChannel = oldData.Channel,
                    _OldAgent = oldData.Agent,
                    _OldPayId = oldData.PayId,
                    _OldCurrencyId = oldData.CurrencyId,
                    _OldSysId = oldData.SysId,
                    _OldAccountNo = oldData.AccountNo,
                    _OldAccountDisplayName = oldData.DisplayName,
                    _NewChannel = bankFinancialTransferAccount.Channel,
                    _NewAgent = bankFinancialTransferAccount.Agent,
                    _NewPayId = bankFinancialTransferAccount.PayId,
                    _NewCurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _NewSysId = bankFinancialTransferAccount.SysId,
                    _NewAccountNo = bankFinancialTransferAccount.AccountNo,
                    _NewAccountDisplayName = bankFinancialTransferAccount.DisplayName,
                });

            parameters.Add("_AffectCount", affectCount, DbType.Int32, ParameterDirection.InputOutput);
            var SpName = "Admin_BankFinancialTransferInfo_ByAccountNo_Set";
            _mainDbClient.Execute(SpName, parameters);

            affectCount = parameters.Get<int?>("_AffectCount") ?? 0;

            return affectCount;
        }


        #endregion

        #region Transfer Info

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> GetFinancialTransferInfoList(GetTransferInfoListParams input,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _IsExport = input.IsExport,
                    _SiteId = input.SiteId,
                    _TransferTypes = input.StrTransferTypes,
                    _TransFromDate = input.StartTime.ToUniversalTime().DateTime,
                    _TransToDate = input.EndTime.ToUniversalTime().DateTime,
                    _FromCurrencyId = input.FromCurrencyId,
                    _ToCurrencyId = input.ToCurrencyId,
                    _FromAccountNo = input.FromAccountNo,
                    _ToAccountNo = input.ToAccountNo,
                    _FromChannel = input.FromChannel,
                    _ToChannel = input.ToChannel,
                    _Reason = input.Reason,
                    _FromAgent = input.FromAgent,
                    _ToAgent = input.ToAgent,
                    _StartIndex = startIndex,
                    _PageSize = pageSize
                });

            parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient
                .Query<BankFinancialTransferInfoTransDetailModel>("Admin_BankFinancialTransferInfo_Get_V3", parameters);

            totalPages = parameters.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameters.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> GetFinancialTransferInfoById(string transferId)
        {
            var parameters = new DynamicParameters(new
            {
                _TransferId = transferId
            });

            var result = _reportDbClient
                .Query<BankFinancialTransferInfoTransDetailModel>("Admin_BankFinancialTransferInfo_GetById", parameters);

            return result;
        }

        public int InsertTransferInfo(BankFinancialTransferInfoModel bankFinancialTransferInfo)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = bankFinancialTransferInfo.TransferId,
                    _TransactionType = bankFinancialTransferInfo.TransactionType,
                    _TransferType = bankFinancialTransferInfo.TransferType,
                    _PlusOrMinus = bankFinancialTransferInfo.PlusOrMinus,
                    _TransDate = bankFinancialTransferInfo.TransDate.DateTime,
                    _Channel = bankFinancialTransferInfo.Channel,
                    _Agent = bankFinancialTransferInfo.Agent,
                    _PayId = bankFinancialTransferInfo.PayId,
                    _SiteId = bankFinancialTransferInfo.SiteId,
                    _SiteName = bankFinancialTransferInfo.SiteName,
                    _CustId = bankFinancialTransferInfo.CustId,
                    _CurrencyId = bankFinancialTransferInfo.CurrencyId,
                    _SysId = bankFinancialTransferInfo.SysId,
                    _AccountNo = bankFinancialTransferInfo.AccountNo,
                    _AccountDisplayName = bankFinancialTransferInfo.AccountDisplayName,
                    _Amount = bankFinancialTransferInfo.Amount,
                    _Fee = bankFinancialTransferInfo.Fee,
                    _Reason = bankFinancialTransferInfo.Reason,
                    _Remark = bankFinancialTransferInfo.Remark,
                    _Creator = bankFinancialTransferInfo.Updator,
                    _Updator = bankFinancialTransferInfo.Updator
                });

            var SpName = "Admin_BankFinancialTransferInfo_Ins_V2";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public int UpdateTransferInfo(BankFinancialTransferInfoModel bankFinancialTransferInfo)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = bankFinancialTransferInfo.TransferId,
                    _TransactionType = bankFinancialTransferInfo.TransactionType,
                    _TransferType = bankFinancialTransferInfo.TransferType,
                    _PlusOrMinus = bankFinancialTransferInfo.PlusOrMinus,
                    _TransDate = bankFinancialTransferInfo.TransDate.DateTime,
                    _Channel = bankFinancialTransferInfo.Channel,
                    _Agent = bankFinancialTransferInfo.Agent,
                    _PayId = bankFinancialTransferInfo.PayId,
                    _SiteId = bankFinancialTransferInfo.SiteId,
                    _SiteName = bankFinancialTransferInfo.SiteName,
                    _CustId = bankFinancialTransferInfo.CustId,
                    _CurrencyId = bankFinancialTransferInfo.CurrencyId,
                    _SysId = bankFinancialTransferInfo.SysId,
                    _AccountNo = bankFinancialTransferInfo.AccountNo,
                    _AccountDisplayName = bankFinancialTransferInfo.AccountDisplayName,
                    _Amount = bankFinancialTransferInfo.Amount,
                    _Fee = bankFinancialTransferInfo.Fee,
                    _Reason = bankFinancialTransferInfo.Reason,
                    _Remark = bankFinancialTransferInfo.Remark,
                    //_UpdateTime = bankFinancialTransferInfo.UpdateTime.DateTime,
                    _Updator = bankFinancialTransferInfo.Updator
                });

            var SpName = "Admin_BankFinancialTransferInfo_Upd_V2";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public int DeleteTransferInfo(string transferId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId
                });

            var SpName = "Admin_BankFinancialTransferInfo_Del";
            return _mainDbClient.Execute(SpName, parameters);
        }

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReport(string siteIds,
                                                                                            string accountNos,
                                                                                            DateTime startDate,
                                                                                            DateTime endDate,
                                                                                            int startIndex,
                                                                                            int pageSize)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = siteIds,
                    _AccountNo = accountNos,
                    _StartDate = startDate,
                    _EndDate = endDate,
                    _StartIndex = startIndex,
                    _PageSize = pageSize
                });

            return _reportDbClient.Query<BankFinancialTransferInfoDetailModel>("Admin_BankFinancialTransferInfoDetail_Get_V2", parameters);
        }

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReport(string siteIds,
                                                                                      string accountNos,
                                                                                    DateTime startDate,
                                                                                    DateTime endDate,
                                                                                    int startIndex,
                                                                                    int pageSize,
                                                                                    out int totalPages,
                                                                                    out int rowCount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = siteIds,
                    _AccountNos = accountNos,
                    _StartDate = startDate,
                    _EndDate = endDate,
                    _StartIndex = startIndex,
                    _PageSize = pageSize
                });

            parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var result = _reportDbClient.Query<BankFinancialTransferInfoDetailModel>("Admin_BankFinancialTransferInfoDetail_Get_V2", parameters);

            totalPages = parameters.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameters.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReportForExport(string siteIds,
                                                                              string accountNos,
                                                                            DateTime startDate,
                                                                            DateTime endDate)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = siteIds,
                    _AccountNos = accountNos,
                    _StartDate = startDate,
                    _EndDate = endDate
                });

            var result = _reportDbClient.Query<BankFinancialTransferInfoDetailModel>("Admin_BankFinancialTransferInfoDetailForExport_Get", parameters);

            return result;
        }


        #endregion

        #region Transfer Info Receipt

        public BankFinancialTransferInfoReceiptModel GetTransferInfoReceipt(string transferId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId
                });
            var SpName = "Admin_BankFinancialTransferInfoReceipt_Get";
            return _reportDbClient.QueryFirst<BankFinancialTransferInfoReceiptModel>(SpName, parameters);
        }

        public int SetTransferInfoReceipt(string transferId, string picFileName, byte[] transferPic = null)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId,
                    _PicFileName = picFileName,
                    _TransferPic = transferPic
                });

            var SpName = "Admin_BankFinancialTransferInfoReceipt_Set";
            return _mainDbClient.Execute(SpName, parameters);
        }

        public int DeleteTransferInfoReceipt(string transferId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId
                });
            var SpName = "Admin_BankFinancialTransferInfoReceipt_Del";
            return _mainDbClient.Execute(SpName, parameters);
        }

        #endregion

        #region Bank Financial Report

        public IEnumerable<BankFinancialDepositSummaryModel> GetDepositSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeZone = timeZone
                });

            var SpName = "Admin_BankFinancialReport_Deposit_Get_V2";
            return _reportDbClient.Query<BankFinancialDepositSummaryModel>(SpName, parameters);
        }

        public IEnumerable<BankFinancialWithdrawalSummaryModel> GetWithdrawalSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeZone = timeZone
                });

            var SpName = "Admin_BankFinancialReport_Withdrawal_Get_V2";
            return _reportDbClient.Query<BankFinancialWithdrawalSummaryModel>(SpName, parameters);
        }

        public IEnumerable<BankFinancialTransferSummaryModel> GetTransferSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeZone = timeZone
                });

            var SpName = "Admin_BankFinancialReport_Transfer_Get_V2";
            return _reportDbClient.Query<BankFinancialTransferSummaryModel>(SpName, parameters);
        }

        public async Task<IEnumerable<FinancialReportResult>> GetFinancialReportDepositAsync(GetFinancialReportParams input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _CurrencyIds = input.CurrencyIds,
                    _Agents = input.Agents,
                    _Channel = input.Channel,
                    _FinancialAccountIds = input.FinancialAccountIds,
                    _StartTime = input.StartTime,
                    _EndTime = input.EndTime
                });

            return await _reportDbClient.QueryAsync<FinancialReportResult>("Admin_BankFinancialReport_Deposit_Get_V3", parameters);
        }

        public async Task<IEnumerable<FinancialReportResult>> GetFinancialReportWithdrawalAsync(GetFinancialReportParams input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _CurrencyIds = input.CurrencyIds,
                    _Agents = input.Agents,
                    _Channel = input.Channel,
                    _FinancialAccountIds = input.FinancialAccountIds,
                    _StartTime = input.StartTime,
                    _EndTime = input.EndTime
                });

            return await _reportDbClient.QueryAsync<FinancialReportResult>("Admin_BankFinancialReport_Withdrawal_Get_V3", parameters);
        }

        public async Task<IEnumerable<FinancialReportResult>> GetFinancialReportTransferAsync(GetFinancialReportParams input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _CurrencyIds = input.CurrencyIds,
                    _Agents = input.Agents,
                    _Channel = input.Channel,
                    _FinancialAccountIds = input.FinancialAccountIds,
                    _StartTime = input.StartTime,
                    _EndTime = input.EndTime
                });

            return await _reportDbClient.QueryAsync<FinancialReportResult>("Admin_BankFinancialReport_Transfer_Get_V3", parameters);
        }

        public async Task<IEnumerable<FinancialAccountOpenBalanceResult>> GetFinancialAccountOpenBalanceAsync(GetFinancialAccountOpenBalanceParams input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _FinancialIds = input.FinancialIds,
                    _StartTime = input.StartTime
                });

            return await _reportDbClient.QueryAsync<FinancialAccountOpenBalanceResult>("Admin_BankFinancialReport_OpenBalance_Get", parameters);
        }

        #endregion

    }
}
