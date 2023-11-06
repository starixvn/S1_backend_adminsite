using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BannerManageService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BannerManageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public int SetBanner(ParamsWithLog<SetRequest> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBanner);
            logParam.Add("_ActionType", string.IsNullOrEmpty(input.Before.BannerName) ? (int)ActionType.Create : (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            if (string.IsNullOrEmpty(input.Before.BannerName))
            {
                logParam.Add("_BeforeDetailInfo", null, DbType.String);
            }
            else
            {
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            }
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            #endregion

            var p = new DynamicParameters();
            p.Add("SiteId", input.SiteId);
            p.Add("BannerName", input.After.BannerName);
            p.Add("BannerType", (int)input.After.BannerType);
            p.Add("ActionMode", (int)input.After.ActionMode);
            p.Add("ShowStatus", (int)input.After.ShowStatus);
            p.Add("StartTime", input.After.StartTime?.DateTime);
            p.Add("EndTime", input.After.EndTime?.DateTime);
            p.Add("BonusCode", input.After.BonusCode);
            p.Add("TpId", input.After.TpId);
            p.Add("GameId", input.After.GameId);
            p.Add("CategoryId", input.After.CategoryId);
            p.Add("UrlRouter", input.After.UrlRouter);
            p.Add("SpecificList", JsonConvert.SerializeObject(input.After.Specific));
            p.Add("Sort", input.After.Sort);
            p.Add("AdminUser", input.Operator);

            var result = _mainDbClient.Execute("Admin_Banner_Set_V3", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public IEnumerable<BannerInfoList> GetBannerList(string siteIds, string bannerTypes,
            string status, string currencies, int startIndex,
            int pageSize, out int totalPages, out int rowCount)
        {

            var props = new DynamicParameters();

            props.Add("_SiteIds", siteIds);
            props.Add("_BannerTypes", bannerTypes);
            props.Add("_Statuses", status);
            props.Add("_CurrencyIds", currencies);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BannerInfoList>("Admin_BannerList_Get_V7", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public BannerInfo GetBanner(int siteId, string bannerName, int currency)
        {
            var result = new BannerInfo();
            var queryResult = _reportDbClient.QueryFirst<dynamic>("Admin_Banner_Get_V3", new { SiteId = siteId, BannerName = bannerName, CurrencyId = currency });

            if (queryResult != null)
            {
                result.SiteId = Convert.ToInt32(queryResult.SiteId);
                result.BannerName = queryResult.BannerName;
                result.BannerType = Convert.ToInt32(queryResult.BannerType);
                result.ActionMode = Convert.ToInt32(queryResult.ActionMode);
                result.ShowStatus = Convert.ToInt32(queryResult.ShowStatus);
                result.StartTime = Convert.ToDateTime(queryResult.StartTime);
                result.EndTime = Convert.ToDateTime(queryResult.EndTime);
                result.BonusCode = queryResult.BonusCode;
                result.TpId = Convert.ToInt32(queryResult.TpId);
                result.GameId = queryResult.GameId;
                result.CategoryId = queryResult.CategoryId;
                result.UrlRouter = queryResult.UrlRouter;
                result.Specific = JsonConvert.DeserializeObject<IEnumerable<BannerDetails>>(queryResult.Specific, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });
                result.Sort = Convert.ToInt32(queryResult.Sort);
            }
            else
            {
                result = null;
            }

            return result;
        }
        public int UpdateBannerStatus(ParamsWithLog<UpdateStatusRequest> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBanner);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            #endregion

            var p = new DynamicParameters();
            p.Add("SiteId", input.SiteId);
            p.Add("BannerName", input.After.BannerName);
            p.Add("Status", (int)input.After.Status);
            p.Add("AdminUser", input.Operator);

            var result = _mainDbClient.Execute("Admin_BannerStatus_Set", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public int DeleteBanner(ParamsWithLog<GetBannerRequest> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBanner);
            logParam.Add("_ActionType", (int)ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            #endregion

            var p = new DynamicParameters();
            p.Add("_SiteId", input.SiteId);
            p.Add("_CurrencyId", input.CurrencyId);
            p.Add("_BannerName", input.Before.BannerName);

            var result = _mainDbClient.Execute("Admin_Banner_Del", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public IEnumerable<HotEventList> GetHotEventList(string siteIds,
            string status, int startIndex,
            int pageSize, out int totalPages, out int rowCount)
        {

            var props = new DynamicParameters();

            props.Add("_SiteIds", siteIds);
            props.Add("_Statuses", status);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<HotEventList>("Admin_HotEventList_Get", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
