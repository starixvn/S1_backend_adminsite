using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GetAllDataController
    {
        #region Fields
        private readonly WithdrawalBLL _withdrawalBll;
        #endregion

        #region Constructors
        public GetAllDataController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
        }
        #endregion

        #region Actions
        [HttpPost]
        public ApiResult<CryptoInfoData> GetCryptoInfoData()
        {

            var result = _withdrawalBll.GetCryptoInfoData();

            return new ApiResult<CryptoInfoData>((int)ErrorCodeEnum.Success, string.Empty, result);
        }
        #endregion
    }
}
