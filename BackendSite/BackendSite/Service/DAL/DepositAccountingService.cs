using System;
using System.Collections.Generic;
using System.Data;
using BackendSite.Controllers.ApiV2.Finance.Deposit;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class DepositAccountingService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public DepositAccountingService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<MySqlClient>>();
            _mainDbClient = new MySqlClient(_configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(_configuration.GetConnectionString("ConnectionStringReport"), logger);
        }

        #endregion
        
        public IEnumerable<DepositAccountingModel> GetDepositAccountingList(string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
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

            var data = _reportDbClient.Query<DepositAccountingModel>(
                "Admin_DepositAccounting_Get", parameter);


            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        
        public DepositAccountingModel GetDepositAccountingByTransId(string transId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TransId", transId);
            var data = _reportDbClient.QueryFirst<DepositAccountingModel>("Admin_DepositAccountingByTransId_Get", parameter);
            
            return data;
        }
        
        public int EditDepositAccounting(ParamsWithLog<DepositAccountingController.EditRequest> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans =  conn.BeginTransaction();
            
            var parameter = new DynamicParameters();
            parameter.Add("_TransId", input.TransId);
            parameter.Add("_AccountNo", input.After.AccountNo);
            parameter.Add("_Memo", input.After.Memo);
            parameter.Add("_Operator", input.Operator);
            var result = conn.Execute("Admin_DepositAccounting_Set", parameter, commandType: CommandType.StoredProcedure);

            if (result > 0)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BetLimit);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog(), DbType.String);
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, conn, trans);
                if (logSuccess > 0)
                {
                    trans.Commit();
                    return result;
                }
            }
            trans.Rollback();
            return result;
        }
        
        public IEnumerable<DepositAccountingModel> ExportDepositAccountingList(
            string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
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

            var data = _reportDbClient.Query<DepositAccountingModel>(
                "Admin_DepositAccountingExport_Get", parameter);
            
            return data;
        }
    }
}