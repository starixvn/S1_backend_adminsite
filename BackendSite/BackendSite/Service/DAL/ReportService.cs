using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class ReportService
    {
        private readonly IConfiguration configuration;

        public ReportService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<CurrencyModel> GetCurrencyList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Common_Currency_Get";
                return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        public Dictionary<string, IEnumerable<PartcialSummeryModel>> GetMainFinancialReport(string siteIds,
                                                                      string currencyIds,
                                                                      long startDate, 
                                                                      long endDate)
        {
            var partcialSummeryModels = new Dictionary<string, IEnumerable<PartcialSummeryModel>>();   

            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate
                });

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
           
            conn.Open();

            #region Get Data StoreProcedure

            var reportDepositList = conn.Query<PartcialSummeryModel>("Admin_ReportDeposit_GroupByDay_Get", 
                                                                     parameters, 
                                                                     commandType: CommandType.StoredProcedure);
            partcialSummeryModels.Add("Deposit", reportDepositList);

            var reportWithdrawalList = conn.Query<PartcialSummeryModel>("Admin_ReportWithdrawal_GroupByDay_Get", 
                                                                        parameters, 
                                                                        commandType: CommandType.StoredProcedure);
            partcialSummeryModels.Add("Withdrawal", reportWithdrawalList);

            var reportAdjustmentList = conn.Query<PartcialSummeryModel>("Admin_ReportAdjustment_GroupByDay_Get", 
                                                                        parameters, 
                                                                        commandType: CommandType.StoredProcedure);
            partcialSummeryModels.Add("Adjustment", reportAdjustmentList);

            var reportBonusList = conn.Query<PartcialSummeryModel>("Admin_ReportBonus_GroupByDay_Get",
                                                                        parameters,
                                                                        commandType: CommandType.StoredProcedure);
            partcialSummeryModels.Add("Bonus", reportBonusList);

            var reportRebateList = conn.Query<PartcialSummeryModel>("Admin_ReportRebate_GroupByDay_Get",
                                                                        parameters,
                                                                        commandType: CommandType.StoredProcedure);
            partcialSummeryModels.Add("Rebate", reportRebateList);

            #endregion

            conn.Close();

            return partcialSummeryModels;
        }

        public IEnumerable<TurnoverAndWinlossSummeryModel> GetTurnoverAndWinlossSummary(string siteIds,
                                                                                        string currencyIds,
                                                                                        long startDate,
                                                                                        long endDate)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate
                });

                string SpName = "Admin_ReportTurnOverAndWinlost_GroupByDay_Get";
                return conn.Query<TurnoverAndWinlossSummeryModel>(SpName, parameters,commandType: CommandType.StoredProcedure);
            }
        }
    }
}
