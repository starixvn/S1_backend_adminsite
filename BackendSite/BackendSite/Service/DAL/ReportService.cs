using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class ReportService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public ReportService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion

        #region Library Methods

        public IEnumerable<CurrencyModel> GetCurrencyList()
        {
            return _reportDbClient.Query<CurrencyModel>("Common_Currency_Get");
        }

        public IEnumerable<ServerInfoModel> GetServerInfoList()
        {
            return _reportDbClient.Query<ServerInfoModel>("Admin_ServerInfoList_Get");
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await _reportDbClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get");
        }

        #endregion

        public Dictionary<string, IEnumerable<PartcialSummeryModel>> GetMainFinancialReport(string siteIds, string currencyIds, string startDate, string endDate ,string timeZone)
        {
            var partialSummeryModels = new Dictionary<string, IEnumerable<PartcialSummeryModel>>();
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeZone = timeZone
                });
            
            #region Get Data StoreProcedure

            var reportDepositList = _reportDbClient.Query<PartcialSummeryModel>("Admin_ReportDeposit_GroupByDay_Get_V2", parameters);
            partialSummeryModels.Add("Deposit", reportDepositList);

            var reportWithdrawalList = _reportDbClient.Query<PartcialSummeryModel>("Admin_ReportWithdrawal_GroupByDay_Get_V2", parameters);
            partialSummeryModels.Add("Withdrawal", reportWithdrawalList);

            var reportAdjustmentList = _reportDbClient.Query<PartcialSummeryModel>("Admin_ReportAdjustment_GroupByDay_Get_V2", parameters);
            partialSummeryModels.Add("Adjustment", reportAdjustmentList);

            var reportBonusList = _reportDbClient.Query<PartcialSummeryModel>("Admin_ReportBonus_GroupByDay_Get_V2", parameters);
            partialSummeryModels.Add("Bonus", reportBonusList);

            var reportRebateList = _reportDbClient.Query<PartcialSummeryModel>("Admin_ReportRebate_GroupByDay_Get_V2", parameters);
            partialSummeryModels.Add("Rebate", reportRebateList);

            #endregion
            
            return partialSummeryModels;
        }

        public IEnumerable<TurnoverAndWinlossSummeryModel> GetTurnoverAndWinLossSummary(string siteIds, string currencyIds, string startDate, string endDate , string timeZone)
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

            return _reportDbClient.Query<TurnoverAndWinlossSummeryModel>("Admin_ReportTurnOverAndWinlost_GroupByDay_Get_V2", parameters);
        }

        public IEnumerable<StatisticalPartialSummeryModel> GetStatisticalReport(string siteIds, string timeZone, string startDate, string endDate)
        {
            var parameters = new DynamicParameters( new
                {
                    _SiteIds = siteIds,
                    _TimeZone = timeZone,
                    _StartDate = startDate,
                    _EndDate = endDate
                });

            return _reportDbClient.Query<StatisticalPartialSummeryModel>("Admin_ReportTurnOverValidBet_Statistical_Get", parameters);
        }

        public IEnumerable<UserOnlinePartcialSummaryModel> GetUserOnlineReport(string siteIds,
            DateTime startTime,
            DateTime endTime,
            string timeZone)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = siteIds,
                    _StartTime = startTime,
                    _EndTime = endTime,
                    _TimeZone = timeZone
                });

            return _reportDbClient.Query<UserOnlinePartcialSummaryModel>("Admin_OnlineReport_Get_V3", parameters);
        }

        public IEnumerable<DepositReportParticalModel> GetDepositReport(string siteIds,
                                                                        string currencyIds,
                                                                        string startDate,
                                                                        string endDate)
        {
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate
                });
            var SpName = "Admin_Report_DepositDetailByDay_Get";
            return _reportDbClient.Query<DepositReportParticalModel>(SpName, parameters);

        }

        public IEnumerable<CustomerStatisticalPartcialSummeryModel> GetCustomerStatisticalReport(string siteIds,
                                                                                                 string currencyIds,
                                                                                                 string startDate,
                                                                                                 string endDate,
                                                                                                 string userName)
        {
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
            return _reportDbClient.Query<CustomerStatisticalPartcialSummeryModel>(SpName, parameters);
        }

        public IEnumerable<TpBetRecordDataModel> GetCustomerDetail(int custId,
                                                                   int gameId,
                                                                   string date)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId,
                    _GameId = gameId,
                    _SelDate = date
                });

            var SpName = "Admin_TpBetRecord_ByCustIdAndGameIdAndDate_Get";
            return _reportDbClient.Query<TpBetRecordDataModel>(SpName, parameters);
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _TpId = input.TpId,
                    _FromTransTime = input.FromTransTime,
                    _ToTransTime = input.ToTransTime,
                    _UserName = input.UserName,
                    _TransId = input.TransId,
                    _PageSize = input.PageSize,
                    _StartIndex = input.StartIndex
                });
            parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var spName = "Admin_TpBetFailureRecord_Get";
            var result = conn.Query<TpFailRecordReport>(spName, parameters, commandType: CommandType.StoredProcedure);
            rowCount = parameters.Get<int>("_RowCount");
            totalPages = parameters.Get<int>("_TotalPages");

            return result;
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameters = new DynamicParameters(
                new
                {
                    _SiteId = input.SiteId,
                    _TpId = input.TpId,
                    _FromTransTime = input.FromTransTime,
                    _ToTransTime = input.ToTransTime,
                    _UserName = input.UserName,
                    _TransId = input.TransId
                });

            var spName = "Admin_TpBetFailureRecordForExport_Get";
            var result = conn.Query<TpFailRecordReport>(spName, parameters, commandType: CommandType.StoredProcedure);
          
            return result;
        }
    }
}
