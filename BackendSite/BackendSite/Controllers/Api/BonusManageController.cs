using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BonusManageController : Controller
    {
        private readonly BonusManageBLL bonusManageBLL;


        public BonusManageController(BonusManageBLL bonusManageBLL)
        {
            this.bonusManageBLL = bonusManageBLL;
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
    }
}