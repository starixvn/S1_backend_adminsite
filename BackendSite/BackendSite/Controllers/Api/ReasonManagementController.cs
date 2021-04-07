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
    public class ReasonManagementController : Controller
    {
        private readonly ReasonManagementBLL reasonManagementBLL;

        public ReasonManagementController(ReasonManagementBLL reasonManagementBLL)
        {
            this.reasonManagementBLL = reasonManagementBLL;
        }

        public ApiResult<IEnumerable<ReasonManagementFuncKey>> GetEventName()
        {

            var apiResult = new ApiResult<IEnumerable<ReasonManagementFuncKey>>();
            apiResult.Data = reasonManagementBLL.GetFuncKey();
            apiResult.ErrorCode = 0;

            return apiResult;

        }
    }
}
