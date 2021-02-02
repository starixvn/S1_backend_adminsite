using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class BannerManageService
    {
        private readonly IConfiguration configuration;

        public BannerManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public int SetBanner(BannerInfo bannerInfo, string user)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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

            return conn.Execute("Admin_Banner_Set", p, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<BannerInfoList> GetBannerList(int SiteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                return conn.Query<BannerInfoList>("Admin_BannerList_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
            }
        }
        public BannerInfo GetBanner(int SiteId, string BannerName)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                BannerInfo result = new BannerInfo();
                var queryresult = conn.QueryFirstOrDefault("Admin_Banner_Get", new { SiteId, BannerName }, commandType: CommandType.StoredProcedure);

                if (queryresult != null)
                {
                    result.SiteId = Convert.ToInt32(queryresult.SiteId);
                    result.BannerName = queryresult.BannerName;
                    result.BannerType = Convert.ToInt32(queryresult.BannerType);
                    result.ActionMode = Convert.ToInt32(queryresult.ActionMode);
                    result.ShowStatus = Convert.ToInt32(queryresult.ShowStatus);
                    result.StartTime = Convert.ToDateTime(queryresult.StartTime);
                    result.EndTime = Convert.ToDateTime(queryresult.EndTime);
                    result.BonusCode = queryresult.BonusCode;
                    result.MatchId = Convert.ToInt32(queryresult.MatchId);
                    result.TpId = Convert.ToInt32(queryresult.TpId);
                    result.GameId = queryresult.GameId;
                    result.UrlRouter = queryresult.UrlRouter;
                    result.SportBattleDateTime = Convert.ToDateTime(queryresult.SportBattleDateTime);
                    result.SportCategoryId = Convert.ToInt32(queryresult.SportCategoryId);
                    result.SportLeaguesId = Convert.ToInt32(queryresult.SportLeaguesId);
                    if (queryresult.SportTeamsId != null)
                    {
                        result.SportTeamsId = JsonConvert.DeserializeObject<IEnumerable<BannerSportTeamsDetails>>(queryresult.SportTeamsId, new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    }
                    else
                    {
                        result.SportTeamsId = null;
                    }
                    result.Specific = JsonConvert.DeserializeObject<IEnumerable<BannerDetails>>(queryresult.Specific, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    result.Sort = Convert.ToInt32(queryresult.Sort);
                }
                else
                {
                    result = null;
                }

                return result;
            }
        }
        public int UpdateBannerStatus(int SiteId, string BannerName, int Status, string User)
        {
            int errorCode = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Execute("Admin_BannerStatus_Set", new { SiteId, BannerName, Status, AdminUser = User }, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                errorCode = 999;
            }

            return errorCode;
        }
    }
}
