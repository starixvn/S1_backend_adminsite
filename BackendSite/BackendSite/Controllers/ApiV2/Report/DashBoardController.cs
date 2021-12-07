using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DashBoardController : Controller
    {
        #region Fields
        
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly DashBoardBLL _dashBoardBll;

        #endregion

        #region Constructor

        public DashBoardController(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _dashBoardBll = provider.GetRequiredService<DashBoardBLL>();
        }

        #endregion

        #region Model

        public class DashBoardReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }
            public int TimeZone { get; set; }
        }

        #endregion
        
        // GET
        /// <summary>
        /// DashBoard/GetDashBoardData 取得首頁儀錶板資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Refresh]
        /// </remarks>
        /// <param name="req">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetDashBoardData(DashBoardReq req)
        {
            var canSearchSite = new List<int>();
            if (req.TimeZone <= 14 && req.TimeZone >= -12)
            {
                _serverInfoBll.GetSiteListByLoginUser().ToList().ForEach(x => canSearchSite.Add(x.SiteId));
                var result = _dashBoardBll.GetDashBoardData(req.SiteId, req.CurrencyId, req.BaseCurrencyId, canSearchSite , req.TimeZone);

                return new ApiResult<DashBoardData>
                {
                    Data = result,
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            
            return new ApiResult<DashBoardData>
            {
                Message ="Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }
    }
}