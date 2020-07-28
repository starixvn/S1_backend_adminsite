using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using MySql.Data.MySqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusManage" })]
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
                var siteList = serverInfoBLL.GetSiteListByLoginUser();
                apiResult.Data = bonusManageBLL.GetPromotionList().Where(b=> siteList.Any(s=> s.SiteId == b.SiteId));
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
        public ApiResult<PromotionInfo> GetPromotion([FromForm]string BonusCode, [FromForm]int SiteId)
        {
            var apiResult = new ApiResult<PromotionInfo>();
            try
            {
                apiResult.Data = bonusManageBLL.GetPromotion(BonusCode, SiteId);
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
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser([FromForm]int? SiteId)
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser()?.Select(x => new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
                if(SiteId != null && apiResult.Data != null && apiResult.Data.Any())
                {
                    apiResult.Data = apiResult.Data.Where(s => s.SiteId == SiteId);
                }
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
        public ApiResult<IEnumerable<PromotionInfo>> SetPromotion([FromForm]bool IsAdd, [FromForm]PromotionInfo Promotion)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                bonusManageBLL.SetPromotion(IsAdd, Promotion);
                apiResult.Data = bonusManageBLL.GetPromotionList();
                apiResult.ErrorCode = 0;
            }
            catch (MySqlException ex)
            {
                var sqlException = ex as MySqlException;
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
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
        public ApiResult<dynamic> UpdatePromotionStatus([FromForm]string BonusCode, [FromForm]int SiteId, [FromForm]int Status)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                apiResult.ErrorCode = bonusManageBLL.SetPromotionStatus(BonusCode, SiteId, Status);
                var promotion = bonusManageBLL.GetPromotion(BonusCode, SiteId);
                apiResult.Data = new { promotion.LastUpdateUser, promotion.LastUpdateTime };
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
        public ApiResult<IEnumerable<PromoCategory>> GetPromoCategories()
        {
            var apiResult = new ApiResult<IEnumerable<PromoCategory>>();

            try
            {
                apiResult.Data = bonusManageBLL.GetPromoCategories();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
    }
}