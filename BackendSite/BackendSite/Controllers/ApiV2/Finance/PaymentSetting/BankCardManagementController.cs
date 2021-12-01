using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankCardManagementController : ControllerBase
    {

        #region Fields

        private readonly SelLangBLL _selLangBll;
        private readonly BankDetailBLL _bankDetailBll;

        #endregion

        #region Contructors

        public BankCardManagementController(IServiceProvider provider)
        {
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
        }

        public class SetRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int SysId { get; set; }
            public string BankAccountName { get; set; }
            public string BankAccountNo { get; set; }
            public string IFSCCode { get; set; }
            public decimal MinAmount { get; set; }
            public decimal MaxAmount { get; set; }
            public decimal DailyLimitAmount { get; set; }
            public int PointLevel { get; set; }
            public int Purpose { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int BankId { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BankInfoList" })]
        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetList(GetListRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<BankDetail>>();
            var depositRequestList = _bankDetailBll.GetLoaclBankList(request.SiteId);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiListResult;
        }

        //重複的邏輯...
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BankInfoList" })]
        //[HttpPost]
        //public ApiResult SetBankCard(bankCardReq bankCardReq)
        //{
        //    var result = _bankDetailBll.SetBankCard(bankCardReq.SiteId,
        //        bankCardReq.SysId,
        //        bankCardReq.BankAccountName,
        //        bankCardReq.BankAccountNo,
        //        bankCardReq.MinAmount,
        //        bankCardReq.MaxAmount,
        //        bankCardReq.DailyLimitAmount,
        //        bankCardReq.PointLevel,
        //        bankCardReq.Purpose,
        //        bankCardReq.Status,
        //        bankCardReq.Sort,
        //        User.Identity.Name);

        //    var apiResult = new ApiResult();
        //    if (result > 0)
        //    {
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //        apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
        //    }
        //    else
        //    {
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
        //        apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
        //    }
        //    return apiResult;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BankInfoList" })]
        [HttpPost]
        public ApiResult Set(SetRequest request)
        {
            if (request.CurrencyId != 61)
            {
                request.IFSCCode = "";
            }

            var result = _bankDetailBll.SetBankCardwithIFSC(request.SiteId,
                request.SysId,
                request.BankAccountName,
                request.BankAccountNo,
                request.IFSCCode,
                request.MinAmount,
                request.MaxAmount,
                request.DailyLimitAmount,
                request.PointLevel,
                request.Purpose,
                request.Status,
                request.Sort,
                User.Identity.Name);

            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BankInfoList" })]
        [HttpPost]
        public ApiResult Delete(DeleteRequest request)
        {
            var result = _bankDetailBll.DelBankCard(request.SiteId,
                request.BankId,
                User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        #endregion

    }
}
