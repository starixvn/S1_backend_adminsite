using BackendSite.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;


namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        private ICustomerBLL _customerBll;

        public CustomerManageController(ICustomerBLL customerBll)
        {
            _customerBll = customerBll;
        }

        /// <summary>
        /// CustomerManage/GetCustomerBasicInfo 取得客戶基本資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public ApiResult<CustomerDto> GetBasicInfo(GetCustomerBasicInfoRequest request)
        {
            var data = _customerBll.GetCustomerBasicInfo(request.CustId);

            var apiResult = new ApiResult<CustomerDto>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
    }
}
