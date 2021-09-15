using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class LatestHighlightVideoManagementBLL
    {
        private readonly LatestHighlightVideoManagementService highlightVideoManagementService;
        private readonly ServerInfoService serverInfoService;

        public LatestHighlightVideoManagementBLL(LatestHighlightVideoManagementService highlightVideoManagementService, ServerInfoService serverInfoService)
        {
            this.highlightVideoManagementService = highlightVideoManagementService;
            this.serverInfoService = serverInfoService;
        }
        public LatestHighlightVideoDataResult GetLatestHighlightVideoData(GetLatestHighlightVideoDataPara para)
        {
            return highlightVideoManagementService.GetLatestHighlightVideoData(para);
        }
        public int SetLatestHighlightVideo(SetLatestHighlightVideoPara para,string User)
        {
            return highlightVideoManagementService.SetLatestHighlightVideo(para, User);
        }
        public IEnumerable<GetSiteLangData> GetSiteLangData(int SiteId)
        {
            var ServerInfo = new ServerInfo();
            ServerInfo = serverInfoService.GetSiteList().ToList().Find(x=> x.SiteId == SiteId);
            var langList = JsonConvert.DeserializeObject<List<string>>(ServerInfo.LangList);
            var result = new List<GetSiteLangData>();
            langList.ForEach(x => result.Add(new GetSiteLangData() { LangId = x,Link= string.Empty,Title = string.Empty}));
            return result;
        }
        public void InsertLatestHighlightVideo(InsrtLatestHighlightVideoPara para, string User)
        {
            var SiteLangData = JsonConvert.DeserializeObject<List<GetSiteLangData>>(para.Detail);
            //remove資料不完全的
            SiteLangData.RemoveAll(x => string.IsNullOrWhiteSpace(x.Link) || string.IsNullOrWhiteSpace(x.Title));
            highlightVideoManagementService.InsertLatestHighlightVideo(para.SiteId, para.Date, SiteLangData, User);
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
