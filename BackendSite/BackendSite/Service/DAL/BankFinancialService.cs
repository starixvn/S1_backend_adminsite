using System;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class BankFinancialService
    {

        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public BankFinancialService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion

        #region Library Methods

        public IEnumerable<CurrencyModel> GetCurrencyList()
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Common_Currency_Get";
            return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankFinancialReasonModel> GetReason(string funcKey)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _FuncKey = funcKey
                });

            var SpName = "Admin_BankFinancialReason_GetByFuncKey";
            return conn.Query<BankFinancialReasonModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public (string, string) NoAssign(string prefix)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters();
            parameters.Add("_Prefix", prefix);
            parameters.Add("_NowDate", direction: ParameterDirection.Output);
            parameters.Add("_NowMaxNo", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var SpName = "Admin_BankFinancialNoAssignForGetNo_Upd";
            var tempresult = conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);

            return ((parameters.Get<string>("_NowDate") ?? string.Empty), (parameters.Get<int?>("_NowMaxNo") ?? 0).ToString());
        }

        #endregion

        #region Transfer Account

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoList(bool isSetting, int siteId, int id)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _IsSetting = isSetting,
                    _SiteId = siteId,
                    _Id = id
                });

            var SpName = "Admin_BankFinancialTransferAccount_Get";
            return _reportDbClient.Query<BankFinancialTransferAccountDetailModel>(SpName, parameters);
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
                    _CreateTime = bankFinancialTransferAccount.CreateTime.DateTime,
                    _Creator = bankFinancialTransferAccount.Creator,
                    _UpdateTime = bankFinancialTransferAccount.UpdateTime.DateTime,
                    _Updator = bankFinancialTransferAccount.Updator
                });

            var SpName = "Admin_BankFinancialTransferAccount_Ins";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public int UpdateTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _Id = bankFinancialTransferAccount.Id,
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
                    _UpdateTime = bankFinancialTransferAccount.UpdateTime.DateTime,
                    _Updator = bankFinancialTransferAccount.Updator
                });

            var SpName = "Admin_BankFinancialTransferAccount_Upd";

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
                    _OperateTime = bankFinancialTransferAccount.UpdateTime.DateTime,
                    _Operator = bankFinancialTransferAccount.Updator,
                    _Action = action
                });

            var SpName = "Admin_BankFinancialTransferAccountLog_Ins";

            return _mainDbClient.Execute(SpName, parameters);
        }

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountList()
        {
            var SpName = "Admin_BankFinancialTransferAccount_GetForList";
            return _reportDbClient.Query<BankFinancialTransferAccountDetailModel>(SpName);
        }

        public int GetTransferDataCount(string accountNo)
        {
            var count = 0;

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
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

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> GetTransferInfoList(bool isSetting,
                                                                                     TransferTypeEnum? transferType,
                                                                                     DateTime? fromDate,
                                                                                     DateTime? toDate,
                                                                                     string fromAccountNo,
                                                                                     string fromAccountDisplayName,
                                                                                     string toAccountNo,
                                                                                     string toAccountDisplayName,
                                                                                     int? reason,
                                                                                     string transferId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _IsSetting = isSetting,
                    _TransferType = transferType,
                    _TransFromDate = fromDate,
                    _TransToDate = toDate,
                    _FromAccountNo = fromAccountNo,
                    _FromAccountDisplayName = fromAccountDisplayName,
                    _ToAccountNo = toAccountNo,
                    _ToAccountDisplayName = toAccountDisplayName,
                    _Reason = reason,
                    _TransferId = transferId
                });

            var SpName = "Admin_BankFinancialTransferInfo_Get_V2";
            return _reportDbClient.Query<BankFinancialTransferInfoTransDetailModel>(SpName, parameters);
        }

        public int InsertTransferInfo(BankFinancialTransferInfoModel bankFinancialTransferInfo)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = bankFinancialTransferInfo.TransferId,
                    _TransactionType = bankFinancialTransferInfo.TransactionType,
                    _TransferType = bankFinancialTransferInfo.TransferType,
                    _PlusOrMinus = bankFinancialTransferInfo.PlusOrMinus,
                    _TransDate = bankFinancialTransferInfo.TransDate,
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
                    _CreateTime = bankFinancialTransferInfo.CreateTime.DateTime,
                    _Creator = bankFinancialTransferInfo.Creator,
                    _UpdateTime = bankFinancialTransferInfo.UpdateTime.DateTime,
                    _Updator = bankFinancialTransferInfo.Updator
                });

            var SpName = "Admin_BankFinancialTransferInfo_Ins";

            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public int UpdateTransferInfo(BankFinancialTransferInfoModel bankFinancialTransferInfo)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = bankFinancialTransferInfo.TransferId,
                    _TransactionType = bankFinancialTransferInfo.TransactionType,
                    _TransferType = bankFinancialTransferInfo.TransferType,
                    _PlusOrMinus = bankFinancialTransferInfo.PlusOrMinus,
                    _TransDate = bankFinancialTransferInfo.TransDate,
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
                    _UpdateTime = bankFinancialTransferInfo.UpdateTime.DateTime,
                    _Updator = bankFinancialTransferInfo.Updator
                });

            var SpName = "Admin_BankFinancialTransferInfo_Upd";

            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public int InsertTransferInfoLog(ActionEnum action, BankFinancialTransferInfoModel bankFinancialTransferInfo)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = bankFinancialTransferInfo.TransferId,
                    _TransactionType = bankFinancialTransferInfo.TransactionType,
                    _TransferType = bankFinancialTransferInfo.TransferType,
                    _PlusOrMinus = bankFinancialTransferInfo.PlusOrMinus,
                    _TransDate = bankFinancialTransferInfo.TransDate,
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
                    //_OperateTime = bankFinancialTransferInfo.UpdateTime.DateTime,
                    _Operator = bankFinancialTransferInfo.Updator,
                    _Action = action
                });

            var SpName = "Admin_BankFinancialTransferInfoLog_Ins_V2";

            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public int DeleteTransferInfo(string transferId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId
                });

            var SpName = "Admin_BankFinancialTransferInfo_Del";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
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

        public int SetTransferInfoReceipt(string transferId, string picFileName, byte[] transferPic)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId,
                    _PicFileName = picFileName,
                    _TransferPic = transferPic
                });

            var SpName = "Admin_BankFinancialTransferInfoReceipt_Set";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public int DeleteTransferInfoReceipt(string transferId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _TransferId = transferId
                });
            var SpName = "Admin_BankFinancialTransferInfoReceipt_Del";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        #endregion

        #region Bank Financial Report

        public IEnumerable<BankFinancialDepositSummaryModel> GetDepositSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
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
            return conn.Query<BankFinancialDepositSummaryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankFinancialWithdrawalSummaryModel> GetWithdrawalSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
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
            return conn.Query<BankFinancialWithdrawalSummaryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankFinancialTransferSummaryModel> GetTransferSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
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
            return conn.Query<BankFinancialTransferSummaryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        #endregion

    }
}
