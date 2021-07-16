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

namespace BackendSite.Service.DAL
{
    public class WithdrawalBankAccountingService
    {
        #region Fields

        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public WithdrawalBankAccountingService(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
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

            var datas = _reportDbClient.Query<WithdrawalInfoAndAccountingListModel>("Admin_WithdrawalInfoAndAccounting_Get", parameter);

         
            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return datas;
        }

        public IEnumerable<WithdrawalBankAccountingModel> GetAccountingListByTransId(string transId)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", transId);

            return _reportDbClient.Query<WithdrawalBankAccountingModel>("Admin_WithdrawalBankAccounting_Get", parameter);
        }

        public GetWithdrawalBankAccountingModel GetById(int id)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", id);

            return _reportDbClient.QueryFirst<GetWithdrawalBankAccountingModel>("Admin_WithdrawalBankAccountingById_Get", parameter);
        }

        public int Insert(WithdrawalBankAccountingModel model, MySqlConnection conn = null,
            MySqlTransaction trans = null)
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

            return conn == null
                ? _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Ins", parameter)
                : conn.Execute("Admin_WithdrawalBankAccounting_Ins", parameter, trans,
                    commandType: CommandType.StoredProcedure);
        }

        public int InsertLog(WithdrawalBankAccountingModel model, MySqlConnection conn = null,
            MySqlTransaction trans = null)
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

            return conn == null
                ? _mainDbClient.Execute("Admin_WithdrawalBankAccountingLog_Ins", parameter)
                : conn.Execute("Admin_WithdrawalBankAccountingLog_Ins", parameter, trans,
                    commandType: CommandType.StoredProcedure);
        }

        public int Update(WithdrawalBankAccountingModel model, MySqlConnection conn = null,
            MySqlTransaction trans = null)
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

            return conn == null
                ? _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Upd", parameter)
                : conn.Execute("Admin_WithdrawalBankAccounting_Upd", parameter, trans,
                    commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// SP Not write yet.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="transId"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int Delete(WithdrawalBankAccountingModel model, MySqlConnection conn = null,
            MySqlTransaction trans = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", model.Id);

            return conn == null
                ? _mainDbClient.Execute("Admin_WithdrawalBankAccounting_Del", parameter)
                : conn.Execute("Admin_WithdrawalBankAccounting_Del", parameter, trans,
                    commandType: CommandType.StoredProcedure);
        }

        #endregion
    }
}