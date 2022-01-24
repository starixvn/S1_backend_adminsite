using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class GameCasinoManagementController : ControllerBase
    {
        #region Fields
        
        private readonly GameCasinoManageBLL _gameCasinoManageBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly LatestHighlightVideoManagementBLL _highlightVideoManagementBll;

        #endregion

        #region Constructor

        public GameCasinoManagementController(IServiceProvider provider)
        {
            _gameCasinoManageBll = provider.GetRequiredService<GameCasinoManageBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _highlightVideoManagementBll = provider.GetRequiredService<LatestHighlightVideoManagementBLL>();
        }

        #endregion

        #region API

         /// <summary>
        /// GameCasinoManagement/SetGameCasinoDetail 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult SetGameCasinoDetail(SetGameCasinoPara request)
        {
            var apiResult = new ApiResult();
            try
            {
                _gameCasinoManageBll.SetGameCasinoDetail(request);
                var beforeStatus = request.Type switch
                {
                    "hot" => request.Hot,
                    "featured" => request.Featured,
                    "status" => request.Status,
                    _ => request.Promotion
                };
                var afterStatus = beforeStatus == 1 ? 2 : 1;
                var log = new GameCasinoDetailLog()
                {
                    GameCasinoId = request.GameCasinoId,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ChangeData = request.Type,
                    Before = beforeStatus,
                    After = afterStatus
                };
                _gameCasinoManageBll.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Update, JsonConvert.SerializeObject(log), User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/InsertGameCasinoDetail 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult InsertGameCasinoDetail(InsertGameCasinoPara request)
        {
            var apiResult = new ApiResult();
            try
            {
                _gameCasinoManageBll.InsertGameCasinoDetail(request);
                _gameCasinoManageBll.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Create, JsonConvert.SerializeObject(request), User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/DeleteGameCasinoDetail 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult DeleteGameCasinoDetail(DeleteGameCasinoPara request)
        {
            var apiResult = new ApiResult();
            try
            {
                _gameCasinoManageBll.DeleteGameCasinoDetail(request);
                _gameCasinoManageBll.AddLog("Marketing/GameCasinoManagement/Detail", ActionType.Delete, JsonConvert.SerializeObject(request), User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/GetTpList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<SiteTpListIfo>>
            {
                Data = _gameCasinoManageBll.GetTpList(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/GameCasinoByTpId 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiListResult<GameCasinoData> GameCasinoByTpId(GameCasinoPara request)
        {
            var apiResult = new ApiListResult<GameCasinoData>();
            try
            {
                var gameCasinoData = _gameCasinoManageBll.GameCasinoByTPIdV2(request);
                apiResult.Data = gameCasinoData.GameCasinoData;
                apiResult.PageNumber = gameCasinoData.PageNumber;
                apiResult.TotalPages = gameCasinoData.TotalPages;
                apiResult.RowCount = gameCasinoData.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/SetGameCasino 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult SetGameCasino(SetGameCasionV2Para request)
        {
            var apiResult = new ApiResult();
            try
            {
                _gameCasinoManageBll.SetGameCasino(request);
                var beforeStatus = request.Type switch
                {
                    "hot" => request.Hot,
                    "featured" => request.Featured,
                    "status" => request.Status,
                    _ => request.Promotion
                };
                var afterStatus = !beforeStatus;
                var log = new GameCasinoLog()
                {
                    GameCasinoId = request.GameCasinoId,
                    ChangeData = request.Type,
                    Before = beforeStatus ? 1 : 2,
                    After = afterStatus ? 1 : 2
                };
                _gameCasinoManageBll.AddLog("Marketing/GameCasinoManagement", ActionType.Update, JsonConvert.SerializeObject(log), User.Identity.Name);
                apiResult.ErrorCode =  (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// GameCasinoManagement/GetGameCasinoDetail 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/GameCasinoManagement" })]
        public ApiResult<IEnumerable<GameCasinoDetailBySite>> GetGameCasinoDetail(GetGameCasinoDetailPara request)
        {

            var apiResult = new ApiResult<IEnumerable<GameCasinoDetailBySite>>();
            try
            {
                var siteList = _serverInfoBll.GetSiteListByLoginUser().ToList();
                apiResult.Data = _gameCasinoManageBll.GetGameCasinoDetailV2(request, siteList);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        #endregion
    }
}