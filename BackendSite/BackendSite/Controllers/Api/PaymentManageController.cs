using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentManageController : Controller
    {
        private readonly PaymentManageBLL paymentManageBLL;
        public PaymentManageController(PaymentManageBLL paymentManageBLL)
        {
            this.paymentManageBLL = paymentManageBLL;
        }

        //public ApiResult GetPayInfoList()
        //{
        //    var result = new ApiResult<IEnumerable<PayList>>();
        //    try
        //    {
        //        result.Data = paymentManageBLL.GetPayInfoList();
        //        result.ErrorCode = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.ErrorCode = 999;
        //        result.Message = "something went wrong.";
        //        result.RawMessage = ex.ToString();
        //    }

        //    return result;
        //}

        public ApiResult GetPayInfoList()
        {
            var result = new ApiResult<FrontEndDatas>();
            try
            {
                result.Data = paymentManageBLL.GetPayInfoList();
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.Message = "something went wrong.";
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}