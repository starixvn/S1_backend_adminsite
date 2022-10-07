using System.Data;
using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class DashBoardService
    {
        private readonly MySqlClient _reportDbClient;

        public DashBoardService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public DashBoardData GetDashBoardData(int siteId,
            int currencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return _reportDbClient.QueryFirst<DashBoardData>("Admin_DashBoard_Get_V2", props);
        }
        
        public DashBoardMemBerDataByMonth GetDashBoardMemberData(int siteId,
            int currencyId,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteId", siteId);

            return _reportDbClient.QueryFirst<DashBoardMemBerDataByMonth>("Admin_DashBoardMember_Get", props);
        }
        
        public IEnumerable<DashBoardMemBerDataByMonth> GetDashBoardMemberDataByMonth(int siteId,
            int currencyId,
            DateTime startDate,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_StartDate", startDate);
            props.Add("_TimeZone", timeZone);
            props.Add("_SiteId", siteId);

            return _reportDbClient.Query<DashBoardMemBerDataByMonth>("Admin_DashBoardMemberGroupByMonth_Get", props);
        }
        
        public DashBoardFinancialDetailData GetDashBoardFinancialData(int siteId,
            int currencyId,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteId", siteId);

            return _reportDbClient.QueryFirst<DashBoardFinancialDetailData>("Admin_DashBoardFinancial_Get", props);
        }
        
        public IEnumerable<DashBoardFinancialDetailData> GetDashBoardFinancialDataByMonth(int siteId,
            int currencyId,
            DateTime startDate,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_StartDate", startDate);
            props.Add("_SiteId", siteId);
            props.Add("_TimeZone", timeZone);

            return _reportDbClient.Query<DashBoardFinancialDetailData>("Admin_DashBoardFinancialGroupByMonth_Get", props);
        }
        
        public IEnumerable<RegistrationPlatformDetailData> GetDashBoardMemberPlatformData(int siteId,
            IEnumerable<int> currencyIds,
            DateTime startDate,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            props.Add("_StartDate", startDate);
            props.Add("_TimeZone", timeZone);
            
            return _reportDbClient.Query<RegistrationPlatformDetailData>("Admin_DashBoardMemberPlatform_Get", props);
        }
        
        public IEnumerable<NewMemberList> GetDashBoardNewMemberList(
            int siteId,
            IEnumerable<int> currencyIds,
            DateTime startDate,
            DateTime endDate,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<NewMemberList>("Admin_DashBoardNewMemberList_Get", parameter);
            
            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;
            
            return data;
        }

        public IEnumerable<TopDepositorsList> GetTopDepositorsList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            DashboardOrderByPropertyEnum orderBy,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            parameter.Add("_BaseCurrencyId", baseCurrencyId);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_OrderBy", (int)orderBy);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<TopDepositorsList>("Admin_DashBoardTopDepositorsList_Get", parameter);
            
            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;
            
            return data;
        }
        
        public IEnumerable<TopWithdrawalList> GetTopWithdrawalList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            DashboardOrderByPropertyEnum orderBy,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            parameter.Add("_BaseCurrencyId", baseCurrencyId);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_OrderBy", (int)orderBy);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<TopWithdrawalList>("Admin_DashBoardTopWithdrawalList_Get", parameter);
            
            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;
            
            return data;
        }
        
        public IEnumerable<TopBonusClaimedList> GetTopBonusClaimedList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            DashboardOrderByPropertyEnum orderBy,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            parameter.Add("_BaseCurrencyId", baseCurrencyId);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_OrderBy", (int)orderBy);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<TopBonusClaimedList>("Admin_DashBoardTopBonusClaimedList_Get", parameter);
            
            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;
            
            return data;
        }
        
        public DashBoardDetailData GetDashBoardTodayData(int siteId,
            int currencyId,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return _reportDbClient.QueryFirst<DashBoardDetailData>("Admin_DashBoard_Today_Get_V2", props);
        }
        
        public DashBoardDetailData GetDashBoardTodayExtraData(int siteId,
            int currencyId,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite,
            int tempTableType)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));
            props.Add("_Type", tempTableType);

            return _reportDbClient.QueryFirst<DashBoardDetailData>("Admin_DashBoard_TodayExtra_Get_V2", props, 60);
        }

        public IEnumerable<DashBoardGamePerMonthDao> GetDashBoardGameData(string siteIdStr,
                                                              string currencyIdStr,
                                                              DateTime startDate,
                                                              DateTime endDate,
                                                              string tpIdStr,
                                                              string categoryStr,
                                                              string gameTypeStr)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIdStr);
            props.Add("_CurrencyIds", currencyIdStr);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TpIds", tpIdStr);
            props.Add("_CategoryIds", categoryStr);
            props.Add("_GameTypeIds", gameTypeStr);

            return _reportDbClient.Query<DashBoardGamePerMonthDao>("Admin_DashBoard_Game_Get", props);
        }

        public IEnumerable<DashBoardGamePerMonthDao> GetDashBoardGameUniqueMemberData(string siteIdStr,
                                                      string currencyIdStr,
                                                      DateTime startDate,
                                                      DateTime endDate,
                                                      string tpIdStr,
                                                      string categoryStr,
                                                      string gameTypeStr)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIdStr);
            props.Add("_CurrencyIds", currencyIdStr);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TpIds", tpIdStr);
            props.Add("_CategoryIds", categoryStr);
            props.Add("_GameTypeIds", gameTypeStr);

            return _reportDbClient.Query<DashBoardGamePerMonthDao>("Admin_DashBoard_Game_UniqueMember_Get", props);
        }
    }
}
