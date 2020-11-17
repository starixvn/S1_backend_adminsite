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
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_Currency_Get";
                return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        public Dictionary<string, IEnumerable<PartcialSummeryModel>> GetMainFinancialReport(string siteIds,
                                                                      int currency,
                                                                      long startDate, 
                                                                      long endDate)
        {
            var partcialSummeryModels = new Dictionary<string, IEnumerable<PartcialSummeryModel>>();   

            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    Currency = currency,
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
    }
}
