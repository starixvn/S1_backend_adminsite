using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using BackendSite.Service.Library;

namespace BackendSite.Service.DAL
{
    public class VideoManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        
        public VideoManagementService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }


        public VideoDataResult GetVideoData(GetVideoDataPara para)
        {
            var result = new VideoDataResult();
            var props = new DynamicParameters();

            props.Add("_SiteId", para.SiteId);
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(para.CurrencyIds));
            props.Add("_LangList", JsonConvert.SerializeObject(para.LangList));
            props.Add("_PageSize", Math.Abs(para.PageSize));
            props.Add("_StartIndex", (Math.Abs(para.PageNumber) - 1) * para.PageSize ,dbType: DbType.Int32);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var tempResult = _reportDbClient.Query<VideoData>("Admin_VideoInfo_Get", props);

            result.VideoDatas = tempResult.ToList();
            result.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        public bool SetVideoInfo(ParamsWithLog<SetVideoPara> para)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            
            var props = new DynamicParameters();
            props.Add("_SiteId", para.SiteId);
            props.Add("_CurrencyId", para.CurrencyId);
            props.Add("_Lang", para.After.LangId);
            props.Add("_Title", para.After.Title);
            props.Add("_Link", para.After.Link);
            props.Add("_Operator", para.Operator);
            var result = _mainDbClient.Execute("Admin_VideoInfo_Set", tran, props) > 0;
            
            if (result)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", para.SiteId);
                logParam.Add("_CurrencyId", para.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.SetVideoInfo);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(para.Before));
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(para.After));
                logParam.Add("_Operator", para.Operator);
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
                if (logResult)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, tran);
                    return logResult;
                }
            }
            
            _mainDbClient.RollbackAndDisposeConnection(conn, tran);
            return false;
        }
        public void InsertLatestHighlightVideo(int siteId,string date, List<GetSiteLangData> siteLangDatas, string user)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            
            foreach (var data in siteLangDatas)
            {
                _mainDbClient.Execute("Admin_LatestHighlightVideo_Int", tran,
                    new { SiteId = siteId, Date = date, Operator = user, data.Title, data.Link, data.LangId });
            }
            _mainDbClient.CommitAndDisposeConnection(conn, tran);
            
        }
    }
}
