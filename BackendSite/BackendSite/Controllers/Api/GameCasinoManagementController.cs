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
        public ApiListResult<GameCasinoData> GameCasinoByTPId(GameCasinoPara gameCasinoPara)
        {
            var apiResult = new ApiListResult<GameCasinoData>();
            try
            {
                var gameCasinoData = gameCasinoManageBLL.GameCasinoByTPId(gameCasinoPara);
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
        public ApiResult<GameCasinoDetail> GetGameCasinoDetail(GameCasinoDetailPara para)
        {
            var apiResult = new ApiResult<GameCasinoDetail>();
            try
            {
                var result = gameCasinoManageBLL.GetGameCasinoDetail(para);
                if(result != null)
                {
                    apiResult.Data = result;
                }
                else
                {
                    apiResult.Data = new GameCasinoDetail
                    {
                        SiteId = 0,
                        CurrencyId =0,
                        GameCasinoId =0,
                        Hot =0,
                        Featured =0,
                        Status =0
                    };
                }
                
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
    }
}
