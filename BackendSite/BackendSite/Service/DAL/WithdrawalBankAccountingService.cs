using BackendSite.Service.Library;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class WithdrawalBankAccountingService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public WithdrawalBankAccountingService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            _mainDbClient = new MySqlClient(_configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(_configuration.GetConnectionString("ConnectionStringReport"), logger);
        }

        #endregion

        #region Methods

        public IEnumerable<WithdrawalInfoAndAccountingListModel> GetWithdrawalInfoAndAccountingList(string siteIds,
            string currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_SiteIds", siteIds);
            parameter.Add("_CurrencyIds", currencyIds);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_TransId", transId);
            parameter.Add("_UserName", userName);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = _reportDbClient.Query<WithdrawalInfoAndAccountingListModel>(
                "Admin_WithdrawalInfoAndAccountingV2_Get", parameter);


            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }

        public IEnumerable<WithdrawalInfoAndAccountingListForExportModel> GetWithdrawalInfoAndAccountingListForExport(
            string siteIds,
            string currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_SiteIds", siteIds);
            parameter.Add("_CurrencyIds", currencyIds);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_TransId", transId);
            parameter.Add("_UserName", userName);

            return _reportDbClient.Query<WithdrawalInfoAndAccountingListForExportModel>(
                "Admin_WithdrawalInfoAndAccountingForExportV2_Get", parameter);
        }

        public IEnumerable<GetWithdrawalBankAccountingModel> GetAccountingListByTransId(string transId, int siteId,int currencyId)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", transId);
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyId", currencyId);

            return _reportDbClient.Query<GetWithdrawalBankAccountingModel>("Admin_WithdrawalBankAccounting_Get",
                parameter);
        }

        public GetWithdrawalBankAccountingModel GetById(int id)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", id);

            return _reportDbClient.QueryFirst<GetWithdrawalBankAccountingModel>(
                "Admin_WithdrawalBankAccountingById_Get", parameter);
        }

        public bool SetFlow(ActionType action, WithdrawalBankAccountingModel model)
        {
            using var connection =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            connection.Open();
            using var transaction = connection.BeginTransaction();
            switch (action)
            {
                case ActionType.Create: //Insert
                    if (Insert(model, connection, transaction) == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    break;
                case ActionType.Update: //Update
                    if (Update(model, connection, transaction) == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    break;
                case ActionType.Delete: //Delete
                    if (Delete(model, connection, transaction) == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    break;
                case ActionType.Read:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (InsertLog((int) action, model, connection, transaction) == 0)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }

        public int Insert(WithdrawalBankAccountingModel model, MySqlConnection conn = null,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Creator", model.Creator);

            return conn?.Execute("Admin_WithdrawalBankAccounting_Ins", parameter, transaction,
                       commandType: CommandType.StoredProcedure) ??
                   _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Ins", parameter);
        }

        public int InsertLog(int action, WithdrawalBankAccountingModel model, MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Mode", action);
            parameter.Add("_Creator", model.Creator);

            return connection?.Execute("Admin_WithdrawalBankAccountingLog_Ins", parameter, transaction,
                       commandType: CommandType.StoredProcedure) ??
                   _mainDbClient.Execute("Admin_WithdrawalBankAccountingLog_Ins", parameter);
        }

        private int Update(WithdrawalBankAccountingModel model, MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", model.Id);
            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Operator", model.Creator);

            return connection?.Execute("Admin_WithdrawalBankAccounting_Upd", parameter, transaction,
                       commandType: CommandType.StoredProcedure) ??
                   _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Upd", parameter);
        }

        private int Delete(WithdrawalBankAccountingModel model, MySqlConnection connection = null,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", model.Id);

            return connection?.Execute("Admin_WithdrawalBankAccounting_Del", parameter, transaction,
                       commandType: CommandType.StoredProcedure) ??
                   _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Del", parameter);
        }

        #endregion
    }
}