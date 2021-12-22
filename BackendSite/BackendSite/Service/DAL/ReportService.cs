using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Common_Currency_Get";
            return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<ServerInfoModel> GetServerInfoList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Admin_ServerInfoList_Get";
            return conn.Query<ServerInfoModel>(SpName, commandType: CommandType.StoredProcedure);
        }
        
        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return await conn.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", commandType: CommandType.StoredProcedure);
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate
                });

            var SpName = "Admin_ReportTurnOverAndWinlost_GroupByDay_Get";
            return conn.Query<TurnoverAndWinlossSummeryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<StatisticalPartcialSummeryModel> GetStatisticalReport(string siteIds,
                                                                                 string timeZone,
                                                                                 string startDate,
                                                                                 string endDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = siteIds,
                    _TimeZone= timeZone,
                    _StartDate = startDate,
                    _EndDate = endDate
                });

            var SpName = "Admin_ReportTurnOverValidBet_Statistical_Get";
            return conn.Query<StatisticalPartcialSummeryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<UserOnlinePartcialSummaryModel> GetUserOnlineReport(string siteIds,
                                                                               string selectDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    SelDate = selectDate
                });

            var SpName = "Admin_OnlineReport_Get";
            return conn.Query<UserOnlinePartcialSummaryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<DepositReportParticalModel> GetDepositReport(string siteIds,
                                                                        string currencyIds,
                                                                        string startDate,
                                                                        string endDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate
                });
            var SpName = "Admin_Report_DepositDetailByDay_Get";
            return conn.Query<DepositReportParticalModel>(SpName, parameters, commandType: CommandType.StoredProcedure);

            //Test Data
            //using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            //{
            //    string SpName = "Temp_EttaReportTest_Get";
            //    return conn.Query<DepositReportParticalModel>(SpName, commandType: CommandType.StoredProcedure);
            //}
        }

        public IEnumerable<CustomerStatisticalPartcialSummeryModel> GetCustomerStatisticalReport(string siteIds,
                                                                                        string currencyIds,
                                                                                        string startDate,
                                                                                        string endDate,
                                                                                        string userName)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    CustomerName = userName
                });

            var SpName = "Admin_ReportTurnOverAndWinlost_CustomerStatistical_Get";
            return conn.Query<CustomerStatisticalPartcialSummeryModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<TpBetRecordDataModel> GetCustomerDetail(int custId,
                                                               int gameId,
                                                               string date)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId,
                    _GameId = gameId,
                    _SelDate = date
                });

            var SpName = "Admin_TpBetRecord_ByCustIdAndGameIdAndDate_Get";
            return conn.Query<TpBetRecordDataModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteId = input.SiteId,
                    TpId = input.TpId,
                    FromTransTime = input.FromTransTime,
                    ToTransTime = input.ToTransTime,
                    UserName = input.UserName,
                    TransId = input.TransId,
                    PageSize = input.PageSize,
                    StartIndex = input.StartIndex
                });
            parameters.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var spName = "Admin_TpBetFailureRecord_Get";
            var result = conn.Query<TpFailRecordReport>(spName, parameters, commandType: CommandType.StoredProcedure);
            rowCount = parameters.Get<int>("RowCount");
            totalPages = parameters.Get<int>("TotalPages");

            return result;
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    SiteId = input.SiteId,
                    TpId = input.TpId,
                    FromTransTime = input.FromTransTime,
                    ToTransTime = input.ToTransTime,
                    UserName = input.UserName,
                    TransId = input.TransId
                });

            var spName = "Admin_TpBetFailureRecordForExport_Get";
            var result = conn.Query<TpFailRecordReport>(spName, parameters, commandType: CommandType.StoredProcedure);
          
            return result;
        }
    }
}
