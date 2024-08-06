using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.GameStatisticalReport;
using BackendSite.Service.BLL;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Report;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class ReportService
    {
        #region Fields
        
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public ReportService(IServiceProvider provider)
        {
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

        public Dictionary<string, IEnumerable<PartcialSummeryModel>> GetMainFinancialReport(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
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

        public IEnumerable<TurnoverAndWinlossSummeryModel> GetTurnoverAndWinLossSummary(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
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

            return _reportDbClient.Query<TurnoverAndWinlossSummeryModel>("Admin_ReportTurnOverAndWinlost_GroupByDay_Get_V3", parameters);
        }

        public IEnumerable<StatisticalPartialSummeryModel> GetStatisticalReport(string siteIds, string timeZone, string startDate, string endDate)
        {
            var parameters = new DynamicParameters(new
            {
                _SiteIds = siteIds,
                _TimeZone = timeZone,
                _StartDate = startDate,
                _EndDate = endDate
            });

            return _reportDbClient.Query<StatisticalPartialSummeryModel>("Admin_ReportTurnOverValidBet_Statistical_Get_V3", parameters);
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

        public IEnumerable<DepositReportParticalModel> GetDepositReport(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
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
            return _reportDbClient.Query<DepositReportParticalModel>("Admin_Report_DepositDetailByDay_Get_V2", parameters);
        }

        public IEnumerable<CustomerStatisticalPartcialSummeryModel> GetCustomerStatisticalReport(string siteIds,
                                                                                                 string currencyIds,
                                                                                                 string startDate,
                                                                                                 string endDate,
                                                                                                 string userName,
                                                                                                 string timeZone)
        {
            var parameters = new DynamicParameters(
                new
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    CustomerName = userName,
                    TimeZone = timeZone
                });

            var SpName = "Admin_ReportTurnOverAndWinlost_CustomerStatistical_Get_V3";
            return _reportDbClient.Query<CustomerStatisticalPartcialSummeryModel>(SpName, parameters);
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = JsonConvert.SerializeObject(input.SiteIds),
                    _CurrencyIds = JsonConvert.SerializeObject(input.CurrencyIds),
                    _TpIds = JsonConvert.SerializeObject(input.TpIds),
                    _FromTransTime = input.FromTransTime,
                    _ToTransTime = input.ToTransTime,
                    _UserName = input.UserName,
                    _TransId = input.TransId,
                    _PageSize = input.PageSize,
                    _StartIndex = input.StartIndex
                });
            parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var spName = "Admin_TpBetFailureRecord_Get_V3";
            var result = _reportDbClient.Query<TpFailRecordReport>(spName, parameters);
            rowCount = parameters.Get<int>("_RowCount");
            totalPages = parameters.Get<int>("_TotalPages");

            return result;
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _SiteIds = JsonConvert.SerializeObject(input.SiteIds),
                    _CurrencyIds = JsonConvert.SerializeObject(input.CurrencyIds),
                    _TpIds = JsonConvert.SerializeObject(input.TpIds),
                    _FromTransTime = input.FromTransTime,
                    _ToTransTime = input.ToTransTime,
                    _UserName = input.UserName,
                    _TransId = input.TransId
                });

            var spName = "Admin_TpBetFailureRecordForExport_Get_V3";
            var result = _reportDbClient.Query<TpFailRecordReport>(spName, parameters);

            return result;
        }

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReport(GetListInput input,
            out int totalPages,
            out int rowCount)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _MultiSiteRequest = JsonConvert.SerializeObject(input.MultiSiteRequest),
                    _BaseCurrencyId = input.BaseCurrencyId,
                    _UserName = input.UserName,
                    _TpUserName = input.TpUserName,
                    _TransId = input.TransId,
                    _EventTypeId = input.EventTypeId,
                    _BonusCode = input.BonusCode,
                    _EventName = input.EventNames != null ? JsonConvert.SerializeObject(input.EventNames) : null,
                    _StartDate = input.StartDate,
                    _EndDate = input.EndDate,
                    _PageSize = input.PageSize,
                    _StartIndex = input.StartIndex
                });
            parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var spName = "Admin_RewardBonusReport_Get_V3";
            var result = _reportDbClient.Query<RewardBonusReportModel>(spName, parameters);
            rowCount = parameters.Get<int>("_RowCount");
            totalPages = parameters.Get<int>("_TotalPages");

            return result;
        }

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReportExport(GetListInput input)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _MultiSiteRequest = JsonConvert.SerializeObject(input.MultiSiteRequest),
                    _BaseCurrencyId = input.BaseCurrencyId,
                    _UserName = input.UserName,
                    _TpUserName = input.TpUserName,
                    _TransId = input.TransId,
                    _EventTypeId = input.EventTypeId,
                    _BonusCode = input.BonusCode,
                    _EventName = input.EventNames != null ? JsonConvert.SerializeObject(input.EventNames) : null,
                    _StartDate = input.StartDate,
                    _EndDate = input.EndDate,
                });

            var spName = "Admin_RewardBonusReport_Export_V3";
            var result = _reportDbClient.Query<RewardBonusReportModel>(spName, parameters);
           
            return result;
        }

        public IEnumerable<GameStatisticalDao> GetGameStatisticalReport(GetListInputModel input)
        {
            var parameters = new DynamicParameters(
              new
              {
                  _SiteId = input.SiteId,
                  _StartDate = input.StartDate,
                  _EndDate = input.EndDate,
              });

            return _reportDbClient.Query<GameStatisticalDao>("Admin_GameStatisticalReport_Get_V2", parameters);
        }
        
        public IEnumerable<PaymentReportDepositDetail> GetPaymentReportDepositDetail(GetPaymentReportParams input,string multiSiteRequest)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _PayIds = input.PayIds,
                    _Types = input.Types,
                    _UserNames = input.UserNames,
                    _StartTime = input.StartTime,
                    _EndTime = input.EndTime,
                    _MultiSiteAndCurrencyIds = multiSiteRequest,
                });

            return _reportDbClient.Query<PaymentReportDepositDetail>("Admin_PaymentReport_Deposit_Get_V2", parameters,600);
        }
        
        public IEnumerable<PaymentReportWithdrawalDetail> GetPaymentReportWithdrawalDetail(GetPaymentReportParams input, string multiSiteRequest)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _PayIds = input.PayIds,
                    _Types = input.Types,
                    _UserNames = input.UserNames,
                    _StartTime = input.StartTime,
                    _EndTime = input.EndTime,
                    _MultiSiteAndCurrencyIds = multiSiteRequest,
                });

            return _reportDbClient.Query<PaymentReportWithdrawalDetail>("Admin_PaymentReport_Withdrawal_Get_V2", parameters);
        }
        
        public IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReport(GetConfiscatedSummaryReportRequest input)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_SiteIds", JsonConvert.SerializeObject(input.SiteIds));
            parameters.Add("_CurrencyIds", JsonConvert.SerializeObject(input.CurrencyIds));
            parameters.Add("_UserName", string.IsNullOrEmpty(input.UserName) ? null : input.UserName);
            parameters.Add("_StartDate", input.StartDate?.UtcDateTime);
            parameters.Add("_EndDate", input.EndDate?.UtcDateTime);
            
            var data = _reportDbClient.Query<ConfiscatedSummaryReportDetail>("Admin_ConfiscatedSummaryReport_Get" ,parameters);
            
            return data;
        }
    }
}
