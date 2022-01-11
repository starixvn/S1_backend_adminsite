using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize]
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

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public List<BankDetailOffStatusEnum> Statuses { get; set; }
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
        /// BankCardManagement/GetList 取得指定品牌銀行卡清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Bank Card Management > Click [Site Name] > List
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BankInfoList" })]
        [HttpPost]
        public ApiListResult<BankDetail> GetListBySiteId(GetListRequest request)
        {
            var apiListResult = new ApiListResult<BankDetail>();
            var statusList = request.Statuses.Select(x => (int)x).ToList();
            var datas = _bankDetailBll.GetLoaclBankList(request.SiteIds, request.CurrencyIds, statusList,
                request.PageNumber, request.PageSize, out int totalPages, out int rowCount);
            apiListResult.Data = datas;
            apiListResult.TotalPages = totalPages;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.RowCount = rowCount;
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
        /// BankCardManagement/Set 設定銀行卡(新增/更新)
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Bank Card Management > Click [Site Name] > List > [Add Bank Card] / [Edit]
        /// 
        /// </remarks>
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
        /// BankCardManagement/Delete 刪除銀行卡
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Bank Card Management > Click [Site Name] > List > [Delete]
        /// 
        /// </remarks>
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
