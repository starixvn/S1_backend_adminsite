using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositAccountingController : ControllerBase
    {
        #region Fields
        
        private readonly SelLangBLL _selLangBll;
        private readonly DepositBLL _depositBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Contructors

        public DepositAccountingController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion
        
        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string TransId { get; set; }
            public string UserName { get; set; }
        }
        
        public class GetByTransIdRequest 
        {
            public string TransId { get; set; }
        }
        
        public class EditRequest 
        {
            public string AccountNo{ get; set; }
            public string Memo{ get; set; }
        }
        
        public class ExportListRequest 
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string TransId { get; set; }
            public string UserName { get; set; }
        }

        #endregion
        
        /// <summary>
        /// DepositAccounting/GetList 取得帳務清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Accounting > [Submit](Query)
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetList(GetListRequest request)
        {
            var endDate = new DateTimeOffset(request.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddDays(1).AddMilliseconds(-1);

            var result = _depositBll.GetDepositAccountingList(request.SiteIds,
                request.CurrencyIds,
                request.StartDate.DateTime,
                endDate.DateTime,
                request.TransId,
                request.UserName,
                request.PageNumber,
                request.PageSize);

            return new ApiListResult<DepositAccountingModel>
            {
                Data = result.Datas,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };
        }

        /// <summary>
        /// DepositAccounting/GetByTransId 取得帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Accounting > [Submit](Query) > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetByTransId(GetByTransIdRequest request)
        {
            var result = _depositBll.GetDepositAccountingByTransId(request.TransId);

            return new ApiResult<DepositAccountingModel>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// DepositAccounting/Edit 編輯帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Accounting > [Submit](Query) > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Edit(ApiRequestWithLog<EditRequest> request)
        {
            var apiResult = new ApiResult();
            
            var beforeData = _depositBll.GetDepositAccountingByTransId(request.TransId);
            if (beforeData == null)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "TransId not found";
                return apiResult;
            }
            request.Before = new EditRequest()
            {
                AccountNo = beforeData.AccountNo,
                Memo = beforeData.Memo
            };
            
            var result = _depositBll.EditDepositAccounting(
                new ParamsWithLog<EditRequest>(request, User.Identity.Name));
            
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        
        /// <summary>
        /// DepositAccounting/ExportList 匯出帳務清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Accounting > [Submit](Export)
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportList(ExportListRequest request)
        {
            var offset = request.StartDate.Offset;
            request.StartDate = new DateTimeOffset(request.StartDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            request.EndDate = new DateTimeOffset(request.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddDays(1).AddMilliseconds(-1);

            var data = _depositBll.ExportDepositAccountingList(JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.TransId,
                request.UserName);
            
            var stream = _excelBll.Export(data, offset: offset);
            return _excelBll.ExcelFile(stream, "DepositAccountList");
        }
    }
}