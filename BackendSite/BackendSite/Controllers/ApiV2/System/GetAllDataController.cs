using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.System
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class GetAllDataController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;

        #endregion

        #region Constructors

        public GetAllDataController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
        }

        #endregion

        #region Actions

        /// <summary>
        /// GetAllData/GetCurrency 取得幣別資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<IEnumerable<CurrencyModel>> GetCurrency()
        {
            var data = _reportBll.GetCurrency();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<CurrencyModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// 取得第三方遊戲大分類清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： [Available Category]
        /// </remarks>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategoryBetLimit>>> GetTpCategoryList()
        {
            var data = await _reportBll.GetTpCategoryList();
            var apiResult = new ApiResult<IEnumerable<TpCategoryBetLimit>>
            {
                Data = data, 
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        #endregion

    }
}
