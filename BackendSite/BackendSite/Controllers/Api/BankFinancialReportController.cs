using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BankFinancialReportController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBLL;
        private readonly BankFinancialBLL _bankFinancialBLL;
        private readonly ExcelBLL _excelBLL;
        private readonly ResponseSettingBLL _responseSettingBLL;

        #endregion

        #region Constructors

        public BankFinancialReportController(IConfiguration configuration,
                                AuthorizeBLL authorizeBLL,
                                BankFinancialBLL bankFinancialBLL,
                                ExcelBLL excelBLL,
                                ResponseSettingBLL responseSettingBLL)
        {
            _authorizeBLL = authorizeBLL;
            _bankFinancialBLL = bankFinancialBLL;
            _excelBLL = excelBLL;
            _responseSettingBLL = responseSettingBLL;
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        public ApiResult<IEnumerable<BankFinancialTransferInfoDetailModel>> GetTransferInfoReport(GetTransferInfoReportReq request)
        {

            var data = _bankFinancialBLL.GetTransferInfoReport(request.SiteId,
                                                              request.AccountNo,
                                                              request.StartDate,
                                                              request.EndDate);

            if (data.Count() == 0)
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

        [HttpPost]
        public IActionResult ExportTransferInfoReport(int SiteId,
                                                    string AccountNo,
                                                    string StartDate,
                                                    string EndDate)
        {
            var data = _bankFinancialBLL.GetTransferInfoReport(SiteId,
                                                                  AccountNo,
                                                                  StartDate,
                                                                  EndDate); ;
            var stream = _excelBLL.Export(data);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TransferInfoReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/BankFinancialReport" })]
        public ApiResult<IEnumerable<BankFinancialDataModel>> GetBankFinancialReport(GetBankFinancialReportReq request)
        {

            var data = _bankFinancialBLL.GetBankFinancialReport(JsonConvert.SerializeObject(request.SiteIds),
                                                              JsonConvert.SerializeObject(request.CurrencyIds),
                                                              request.StartDate,
                                                              request.EndDate);

            if (data.Count() == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<BankFinancialDataModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [HttpPost]
        public IActionResult ExportBankFinancialReport(string SiteIds,
                                                       string CurrencyIds,
                                                       string StartDate,
                                                       string EndDate)
        {
            var data = _bankFinancialBLL.GetBankFinancialReport(SiteIds,
                                                                CurrencyIds,
                                                                StartDate,
                                                                EndDate);
            var stream = _excelBLL.Export(data);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BankFinancialReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }


        #endregion

    }
}
