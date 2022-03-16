using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Controllers.ApiV2.CMS.Setting;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

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
        public int SetBanner(BannerController.SetBannerReq bannerInfo, string user)
        {
            var p = new DynamicParameters();
            p.Add("BannerName", bannerInfo.BannerName);
            p.Add("SiteId", bannerInfo.SiteId);
            p.Add("BannerType", bannerInfo.BannerType);
            p.Add("ActionMode", bannerInfo.ActionMode);
            p.Add("ShowStatus", bannerInfo.ShowStatus);
            p.Add("UnixStartTime", bannerInfo.UnixStartTime);
            p.Add("UnixEndTime", bannerInfo.UnixEndTime);
            p.Add("BonusCode", bannerInfo.BonusCode);
            p.Add("MatchId", bannerInfo.MatchId);
            p.Add("TpId", bannerInfo.TpId);
            p.Add("GameId", bannerInfo.GameId);
            p.Add("UrlRouter", bannerInfo.UrlRouter);
            p.Add("UnixSportBattleDateTime", bannerInfo.UnixSportBattleDateTime);
            p.Add("SportCategoryId", bannerInfo.SportCategoryId);
            p.Add("SportLeaguesId", bannerInfo.SportLeaguesId);
            p.Add("SportTeamsId", JsonConvert.SerializeObject(bannerInfo.SportTeamsId));
            p.Add("SpecificList", JsonConvert.SerializeObject(bannerInfo.Specific));
            p.Add("Sort", bannerInfo.Sort);
            p.Add("AdminUser", user);
            
            return _mainDbClient.Execute("Admin_Banner_Set_V2", p);
        }
        public IEnumerable<BannerInfoList> GetBannerList(string siteIds, string bannerTypes,
            string status,string currencies ,int startIndex,
            int pageSize,out int totalPages,out int rowCount)
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

            var result = _reportDbClient.Query<BannerInfoList>("Admin_BannerList_Get_V5", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public BannerInfo GetBanner(int siteId, string bannerName , int currency)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = new BannerInfo();
            var queryResult = conn.QueryFirstOrDefault("Admin_Banner_Get_V3", new {SiteId = siteId, BannerName = bannerName , CurrencyId = currency }, commandType: CommandType.StoredProcedure);

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
                result.MatchId = Convert.ToInt32(queryResult.MatchId);
                result.TpId = Convert.ToInt32(queryResult.TpId);
                result.GameId = queryResult.GameId;
                result.UrlRouter = queryResult.UrlRouter;
                result.SportBattleDateTime = queryResult.SportBattleDateTime == null ? null : Convert.ToDateTime(queryResult.SportBattleDateTime);
                result.SportCategoryId = Convert.ToInt32(queryResult.SportCategoryId);
                result.SportLeaguesId = Convert.ToInt32(queryResult.SportLeaguesId);
                if (queryResult.SportTeamsId != null)
                {
                    result.SportTeamsId = JsonConvert.DeserializeObject<IEnumerable<BannerSportTeamsDetails>>(queryResult.SportTeamsId, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                }
                else
                {
                    result.SportTeamsId = null;
                }
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
        public int UpdateBannerStatus(int siteId, string bannerName, int status, string user)
        {
            var errorCode = 0;
            try
            {
                _mainDbClient.Execute("Admin_BannerStatus_Set", new DynamicParameters(new {SiteId = siteId, BannerName = bannerName, Status = status, AdminUser = user }));
            }
            catch (Exception)
            {
                errorCode = 999;
            }

            return errorCode;
        }
    }
}
