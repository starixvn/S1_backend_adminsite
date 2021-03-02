using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameCasinoManagementController : ControllerBase
    {
        private readonly GameCasinoManageBLL gameCasinoManageBLL;

        public GameCasinoManagementController(GameCasinoManageBLL gameCasinoManageBLL)
        {
            this.gameCasinoManageBLL = gameCasinoManageBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult SetGameCasinoDetail(SetGameCasionPara para) 
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.SetGameCasinoDetail(para);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult InsertGameCasinoDetail(InsertGameCasionPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.InsertGameCasinoDetail(para);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult DeleteGameCasinoDetail(DeleteGameCasionPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.DeleteGameCasinoDetail(para);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<SiteTpListIfo>>();
            apiResult.Data = gameCasinoManageBLL.GetTpList();
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult SetGameCasino(SetGameCasionV2Para para)
        {
            var apiResult = new ApiResult();
            try
            {
                gameCasinoManageBLL.SetGameCasino(para);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult<IEnumerable<GameCasinoDetailData>> GetGameCasinoDetailV2(GetGameCasinoDetailPara para)
        {

            var apiResult = new ApiResult<IEnumerable<GameCasinoDetailData>>();
            try
            {
                apiResult.Data = gameCasinoManageBLL.GetGameCasinoDetail(para);
                apiResult.ErrorCode = 0;
            }
            catch(Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}
