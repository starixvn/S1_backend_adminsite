using System;
using System.Collections.Generic;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        
        public int EditDepositAccounting(string transId, string accountNo ,string memo ,string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TransId", transId);
            parameter.Add("_AccountNo", accountNo);
            parameter.Add("_Memo", memo);
            parameter.Add("_Operator", Operator);
            
            var data = _mainDbClient.Execute("Admin_DepositAccounting_Set", parameter);
            
            return data;
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