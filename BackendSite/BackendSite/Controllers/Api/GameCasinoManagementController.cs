using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GameCasinoManagementController : ControllerBase
    {
        private readonly GameCasinoManageBLL gameCasinoManageBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public GameCasinoManagementController(GameCasinoManageBLL gameCasinoManageBLL, ServerInfoBLL serverInfoBLL)
        {
            this.gameCasinoManageBLL = gameCasinoManageBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult SetGameCasinoDetail(SetGameCasinoPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.SetGameCasinoDetail(para);
                var beforeStatus = para.Type == "hot" ? para.Hot : para.Type == "featured" ? para.Featured : para.Type == "status" ? para.Status : para.Promotion;
                var afterStatus = beforeStatus == 1 ? 2 : 1;
                var log = new GameCasinoDetailLog()
                {
                    GameCasinoId = para.GameCasinoId,
                    SiteId = para.SiteId,
                    CurrencyId = para.CurrencyId,
                    ChangeData = para.Type,
                    Before = beforeStatus,
                    After = afterStatus
                };
                gameCasinoManageBLL.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Update, JsonConvert.SerializeObject(log), User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult InsertGameCasinoDetail(InsertGameCasinoPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.InsertGameCasinoDetail(para);
                gameCasinoManageBLL.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Create, JsonConvert.SerializeObject(para), User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult DeleteGameCasinoDetail(DeleteGameCasinoPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.DeleteGameCasinoDetail(para);
                gameCasinoManageBLL.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Delete, JsonConvert.SerializeObject(para), User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<SiteTpListIfo>>
            {
                Data = gameCasinoManageBLL.GetTpList(),
                ErrorCode = 0
            };

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiListResult<GameCasinoData> GameCasinoByTPIdV2(GameCasinoPara gameCasinoPara)
        {
            var apiResult = new ApiListResult<GameCasinoData>();
            try
            {
                var gameCasinoData = gameCasinoManageBLL.GameCasinoByTPIdV2(gameCasinoPara);
                apiResult.Data = gameCasinoData.GameCasinoData;
                apiResult.PageNumber = gameCasinoData.PageNumber;
                apiResult.TotalPages = gameCasinoData.TotalPages;
                apiResult.RowCount = gameCasinoData.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult SetGameCasino(SetGameCasionV2Para para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.SetGameCasino(para);
                var beforeStatus = para.Type == "hot" ? para.Hot : para.Type == "featured" ? para.Featured : para.Type == "status" ? para.Status : para.Promotion;
                var afterStatus = !beforeStatus;
                var log = new GameCasinoLog()
                {
                    GameCasinoId = para.GameCasinoId,
                    ChangeData = para.Type,
                    Before = beforeStatus ? 1 : 2,
                    After = afterStatus ? 1 : 2
                };
                gameCasinoManageBLL.AddLog("Marketing/GameCasinoManagement", ActionType.Update, JsonConvert.SerializeObject(log), User.Identity.Name);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult<IEnumerable<GameCasinoDetailBySite>> GetGameCasinoDetailV3(GetGameCasinoDetailPara para)
        {

            var apiResult = new ApiResult<IEnumerable<GameCasinoDetailBySite>>();
            try
            {
                var SiteList = serverInfoBLL.GetSiteListByLoginUser().ToList();
                apiResult.Data = gameCasinoManageBLL.GetGameCasinoDetailV2(para, SiteList);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}
