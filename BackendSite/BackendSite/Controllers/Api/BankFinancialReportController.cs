using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BackendSite.Service.Library;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BankFinancialReportController : ControllerBase
    {
        #region Fields

        private readonly BankFinancialBLL _bankFinancialBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public BankFinancialReportController(BankFinancialBLL bankFinancialBLL,
                                ExcelBLL excelBLL
            )
        {
            _bankFinancialBLL = bankFinancialBLL;
            _excelBLL = excelBLL;
        }

        #endregion

        #region API Model

        public class GetTransferInfoReportReq
        {
            public int SiteId
            {
                get;
                set;
            }

            public string AccountNo
            {
                get;
                set;
            }

            public string StartDate
            {
                get;
                set;
            }

            public string EndDate
            {
                get;
                set;
            }
        }

        public class GetBankFinancialReportReq
        {
            public IEnumerable<int> SiteIds
            {
                get;
                set;
            }

            public IEnumerable<int> CurrencyIds
            {
                get;
                set;
            }

            public string StartDate
            {
                get;
                set;
            }

            public string EndDate
            {
                get;
                set;
            }
        }


        #endregion

        #region Action

        #region TransferInfo Report

        /// <summary>
        /// BankFinancialReport/GetTransferInfoReport 取得轉帳報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Transfer Info Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        public ApiResult<IEnumerable<BankFinancialTransferInfoDetailModel>> GetTransferInfoReport(GetTransferInfoReportReq request)
        {

            var siteList = new List<int>(); siteList.Add(request.SiteId);
            var accountNoList = new List<string>(); accountNoList.Add(request.AccountNo);

            //因為現在資料的transDate是以+8存在DB，固這邊站不轉換時間。
            var startDate = Convert.ToDateTime(request.StartDate);
            var endDate = Convert.ToDateTime(request.EndDate);

            var data = _bankFinancialBLL.GetTransferInfoReport(siteList,
                                                              accountNoList,
                                                              startDate,
                                                              endDate,
                                                              1,
                                                              10000, out _, out _);

            data.ToList().ForEach(s => s.CreateTime = new DateTimeOffset(s.CreateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
            data.ToList().ForEach(s => s.UpdateTime = new DateTimeOffset(s.UpdateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<BankFinancialTransferInfoDetailModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// BankFinancialReport/ExportTransferInfoReport 匯出轉帳報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Transfer Info Report > [Export]
        /// 
        /// </remarks>
        /// <param name="SiteId"></param>
        /// <param name="AccountNo"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportTransferInfoReport(int SiteId,
                                                    string AccountNo,
                                                    string StartDate,
                                                    string EndDate)
        {
            var siteList = new List<int>(); siteList.Add(SiteId);
            var accountNoList = new List<string>(); accountNoList.Add(AccountNo);

            var startDate = Convert.ToDateTime(StartDate);
            var endDate = Convert.ToDateTime(EndDate);

            var data = _bankFinancialBLL.GetTransferInfoReportForExport(siteList,
                                                              accountNoList,
                                                              startDate,
                                                              endDate);

            data.ToList().ForEach(s => s.CreateTime = new DateTimeOffset(s.CreateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
            data.ToList().ForEach(s => s.UpdateTime = new DateTimeOffset(s.UpdateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            var stream = _excelBLL.ExportV2(data);

            return _excelBLL.ExcelFile(stream, "TransferInfoReport");
        }

        #endregion

        #endregion
    }
}
