using BackendSite.Service.Model;
using Dapper;
using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class VipLevelService : IVipLevelService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public VipLevelService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<VipLevelSetting> GetVipLevelSetting(int siteId)
        {
            var vipLevelSetting = _mainDbClient.Query<VipLevelSetting>("Admin_VipLevel_Get", new DynamicParameters(new { SiteId = siteId }));
            return vipLevelSetting;
        }

        public async Task<IEnumerable<VipLevelSetting>> GetVipLevelSettingAsync(int siteId)
        {
            return await _reportDbClient.QueryAsync<VipLevelSetting>("Admin_VipLevel_Get",
                new DynamicParameters(new { SiteId = siteId }));
        }

        public VipLevelInfo GetVipLevelInfo(int CustId)
        {
            var vipLevelInfo = _reportDbClient.QueryFirst<VipLevelInfo>("Admin_CustVipLevel_Get_V3", new { CustId });
            return vipLevelInfo;
        }
        
        public async Task<VipLevelInfoDto> GetVipLevelInfoAsync(int custId)
        {
            return await _reportDbClient.QueryFirstAsync<VipLevelInfoDto>("Admin_CustVipLevel_Get_V4",
                new DynamicParameters(new { CustId = custId }));
        }

        public IEnumerable<VipLevelLog> GetVipLevelLog(int custId, int startIndex, int pageSize, out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<VipLevelLog>("Admin_CustVipLevelLog_Get_V4", props, commandType: CommandType.StoredProcedure);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        
        public async Task<IEnumerable<VipLevelLogDto>> GetVipLevelLogAsync(int custId)
        {
            return await _reportDbClient.QueryAsync<VipLevelLogDto>("Admin_CustVipLevelLog_Get_V5",
                new DynamicParameters(new { CustId = custId }));
        }

        public int SetVipLevel(int SiteId, int CustId, string UserName, int VipLevel, string Memo, string Operator)
        {
            var result = _mainDbClient.Execute("Admin_CustVipLevel_Set", new { SiteId, CustId, UserName, VipLevel, Memo, Operator });
            return result;
        }
        
        public async Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string memo, string updator)
        {
            var p = new DynamicParameters(new
            {
                SiteId = siteId, CustId = custId, UserName = userName, 
                VipLevel = vipLevel, Memo = memo, Operator = updator
            });
            return await _mainDbClient.ExecuteAsync("Admin_CustVipLevel_Set", p);
        }

        public int SetVipLevelManually(int SiteId, int CustId, string UserName, int Manually, string Memo,
            string Operator)
        {
            var result = _mainDbClient.Execute("Admin_CustVipLevelManually_Set", new { SiteId, CustId, UserName, Manually, Memo, Operator });
            return result;
        }

        public async Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string memo, string updator)
        {
            var p = new DynamicParameters(new
            {
                SiteId = siteId, CustId = custId, UserName = userName, 
                Manually = manually, Memo = memo, Operator = updator
            });
            return await _mainDbClient.ExecuteAsync("Admin_CustVipLevelManually_Set", p);
        }

        public Task<int> SetAutoCustVipExclusionAsync(int siteId, int custId, string userName, int exclusive)
        {
            return _mainDbClient.ExecuteAsync("Admin_AutoCustVipExclusion_Set", new
            {
                _SiteId = siteId,
                _CustId = custId,
                _UserName = userName,
                _Exclusive = exclusive
            });
        }

        public VipSettingModel GetVipSetting(int vipType, int siteId)
        {
            var props = new DynamicParameters();
            props.Add("_Status", vipType);
            props.Add("_SiteId", siteId);
            return _reportDbClient.QueryFirst<VipSettingModel>("Admin_VipSetting_Get_V2", props);
        }

        public IEnumerable<VipUpgradeReviewDataModel> GetVipUpgradeReviewList(int siteId,
            string calculateMonth,
            string selectStatus,
            string userName,
            int pageSize,
            int startIndex,
            string currencyIds,
            out int totalPages,
            out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CalculateMonth", calculateMonth);
            props.Add("_SelectStatus", selectStatus);
            props.Add("_UserName", userName);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<VipUpgradeReviewDataModel>("Admin_VipUpgradeReview_Get_V3", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool UpdateVipUpgradeReviewFlow(string custIds, string yearMonth, int newStatus, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_CustIds", custIds);
            props.Add("_CalculateMonth", yearMonth);
            props.Add("_Status", newStatus);
            props.Add("_LastUpdateUser", updater);
            _mainDbClient.Execute("Admin_VipUpgradeReviewV2_Set", props);

            return true;
        }

        public IEnumerable<VipDowngradeReviewDataModel> GetVipDowngradeReviewList(int siteId,
            string calculateYearMonth,
            string selectStatus,
            string currencyId,
            string userName,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_SelectStatus", selectStatus);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_UserName", userName);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<VipDowngradeReviewDataModel>("Admin_VipDowngradeReview_Get_V3", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool UpdateVipDowngradeReviewFlow(string custIdList, string calculateYearMonth, int newStatus, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_CustIdList", custIdList);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_Status", newStatus);
            props.Add("_LastUpdateUser", updater);
            _mainDbClient.Execute("Admin_VipDowngradeReview_Set", props);
            
            return true;
        }

        #region Private Methods

        #endregion
    }
}
