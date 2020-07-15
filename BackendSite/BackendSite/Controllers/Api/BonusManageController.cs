using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace BackendSite.Controllers.Api
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BonusManageController : Controller
    {
        private readonly BonusManageBLL bonusManageBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public BonusManageController(BonusManageBLL bonusManageBLL, ServerInfoBLL serverInfoBLL)
        {
            this.bonusManageBLL = bonusManageBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> GetPromotionList()
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                apiResult.Data = bonusManageBLL.GetPromotionList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<PromotionInfo> GetPromotion([FromForm]string BonusCode)
        {
            var apiResult = new ApiResult<PromotionInfo>();
            try
            {
                apiResult.Data = bonusManageBLL.GetPromotion(BonusCode);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser()
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser()?.Select(x => new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList });
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> SetPromotion([FromForm]bool IsAdd, PromotionInfo Promotion)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                var result = bonusManageBLL.SetPromotion(IsAdd, Promotion);
                apiResult.ErrorCode = result;
                if (result == 0)
                {
                    apiResult.Data = bonusManageBLL.GetPromotionList();
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult UpdatePromotionStatus([FromForm]string BonusCode, [FromForm]int SiteId, [FromForm]int Status)
        {
            var apiResult = new ApiResult();
            try
            {
                apiResult.ErrorCode = bonusManageBLL.SetPromotionStatus(BonusCode, SiteId, Status);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
    }
}