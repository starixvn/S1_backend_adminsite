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

        #region Fields

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public ReportService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Library Methods

        public IEnumerable<CurrencyModel> GetCurrencyList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Common_Currency_Get";
                return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<ServerInfoModel> GetServerInfoList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_ServerInfoList_Get";
                return conn.Query<ServerInfoModel>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        #endregion

        public Dictionary<string, IEnumerable<PartcialSummeryModel>> GetMainFinancialReport(string siteIds,
                                                                                            string currencyIds,
                                                                                            string startDate,
                                                                                            string endDate)
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
                                                                                        string startDate,
                                                                                        string endDate)
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
                return conn.Query<TurnoverAndWinlossSummeryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<StatisticalPartcialSummeryModel> GetStatisticalReport(string siteIds,
                                                                                 string startDate,
                                                                                 string endDate)
        {

            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    StartDate = startDate,
                    EndDate = endDate
                });

                string SpName = "Admin_ReportTurnOverValidBet_Statistical_Get";
                return conn.Query<StatisticalPartcialSummeryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<UserOnlinePartcialSummaryModel> GetUserOnlineReport(string siteIds,
                                                                               string selectDate)
        {

            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    SelDate = selectDate
                });

                string SpName = "Admin_OnlineReport_Get";
                return conn.Query<UserOnlinePartcialSummaryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<DepositReportParticalModel> GetDepositReport(string siteIds,
                                                                        string currencyIds,
                                                                        string startDate,
                                                                        string endDate)
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
                string SpName = "Admin_Report_DepositDetailByDay_Get";
                return conn.Query<DepositReportParticalModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
            
            //Test Data
            //using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            //{
            //    string SpName = "Temp_EttaReportTest_Get";
            //    return conn.Query<DepositReportParticalModel>(SpName, commandType: CommandType.StoredProcedure);
            //}
        }

    }
}
