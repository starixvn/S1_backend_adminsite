using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class VideoManagementBLL
    {
        private readonly VideoManagementService videoManagementService;
        private readonly ServerInfoService serverInfoService;

        public VideoManagementBLL(VideoManagementService highlightVideoManagementService, ServerInfoService serverInfoService)
        {
            this.videoManagementService = highlightVideoManagementService;
            this.serverInfoService = serverInfoService;
        }
        public VideoDataResult GetVideoData(GetVideoDataPara para)
        {
            return videoManagementService.GetVideoData(para);
        }
        public bool SetVideoInfo(ParamsWithLog<SetVideoPara> input)
        {
            return videoManagementService.SetVideoInfo(input);
        }
        public IEnumerable<GetSiteLangData> GetSiteLangData(int siteId)
        {
            var serverInfo = serverInfoService.GetSiteList().ToList().Find(x=> x.SiteId == siteId);
            var langList = JsonConvert.DeserializeObject<List<string>>(serverInfo?.LangList);
            var result = new List<GetSiteLangData>();
            langList.ForEach(x => result.Add(new GetSiteLangData() { LangId = x,Link= string.Empty,Title = string.Empty}));
            return result;
        }
        public void InsertLatestHighlightVideo(InsertVideoPara para, string user)
        {
            var siteLangData = JsonConvert.DeserializeObject<List<GetSiteLangData>>(para.Detail);
            //remove資料不完全的
            siteLangData.RemoveAll(x => string.IsNullOrWhiteSpace(x.Link) || string.IsNullOrWhiteSpace(x.Title));
            videoManagementService.InsertLatestHighlightVideo(para.SiteId, para.Date, siteLangData, user);
        }

        public void AddLog(string menu, ActionType actionType, string detailInfo, string user)
        {
            var addAdminOperateLog = new AddAdminOperateLog
            {
                Menu = menu,
                ActionType = actionType,
                DetailInfo = detailInfo,
                Operator = user
            };
            serverInfoService.AddAdminOperateLog(addAdminOperateLog);
        }
    }
}
