using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TeleCSDController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL _custInfoBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public TeleCSDController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class TeleCsdReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> Currency { get; set; }
            public int Deposit { get; set; }
            public int CallOut { get; set; }
            public int DormantUser { get; set; }
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

        }
        public class TeleCsdUpdateReq
        {
            public int CustId { get; set; }
            public int SiteId { get; set; }
        }

        public class ExportTeleCsdDataReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> Currency { get; set; }
            public int Deposit { get; set; }
            public int CallOut { get; set; }
            public int DormantUser { get; set; }
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

        }

        #endregion

        #region Action

        /// <summary>
        /// TeleCSD/GetTeleCSD 取得客服致電資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > [Submit](Query)
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public ApiListResult<CustTele> GetTeleCSD(TeleCsdReq req)
        {
            var searchResult = _custInfoBll.GetTeleCSD(JsonConvert.SerializeObject(req.SiteIds),
                                                     JsonConvert.SerializeObject(req.Currency),
                                                     req.StartDate,
                                                     req.EndDate,
                                                     req.Deposit,
                                                     req.DormantUser,
                                                     req.CallOut,
                                                     req.PageNumber,
                                                     req.PageSize);

            if (searchResult.CustTeleList.Count == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                foreach (var list in searchResult.CustTeleList)
                {
                    list.RegTime = new DateTimeOffset(list.RegTime.DateTime, TimeSpan.Zero);
                }
                return new ApiListResult<CustTele>
                {
                    Data = searchResult.CustTeleList,
                    TotalPages = searchResult.TotalPages,
                    PageNumber = searchResult.PageNumber,
                    RowCount = searchResult.RowCount,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// TeleCSD/UpdateCustomerTele 更新客服致電資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > Submit(Query) > [Complete once]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public ApiResult UpdateCustomerTele(TeleCsdUpdateReq req)
        {
            if (_custInfoBll.UpdateCustomerTele(req.SiteId, req.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
        }

        /// <summary>
        /// TeleCSD/ExportTeleCSDData 匯出客服致電資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > Submit(Query) > [Export Excel]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns>檔案</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public IActionResult ExportTeleCSDData(ExportTeleCsdDataReq req)
        {
            var exportData = _custInfoBll.GetTeleCSDExportData(JsonConvert.SerializeObject(req.SiteIds),
                                                              JsonConvert.SerializeObject(req.Currency),
                                                              req.StartDate,
                                                              req.EndDate,
                                                              req.Deposit,
                                                              req.DormantUser,
                                                              req.CallOut);
            var custTeleExports = exportData.ToList();
            foreach (var list in custTeleExports)
            {
                list.RegDate = new DateTimeOffset(list.RegDate.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.Export(custTeleExports);
            return _excelBll.ExcelFile(stream, "TeleCSD");
        }

        #endregion
    }
}