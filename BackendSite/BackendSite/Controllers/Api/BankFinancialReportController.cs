using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
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
        public IActionResult GetTransferInfoReport(GetTransferInfoReportReq request)
        {
            try
            {
                var data = _bankFinancialBLL.GetTransferInfoReport(request.SiteId,
                                                                  request.AccountNo,
                                                                  request.StartDate,
                                                                  request.EndDate);
                if (data.Count() == 0)
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<BankFinancialTransferInfoDetailModel>>(Service.Model.enums.ErrorMessageCodeEnum.NoData,
                                                                         data));
                }
                else
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<BankFinancialTransferInfoDetailModel>>(Service.Model.enums.ErrorMessageCodeEnum.Success,
                                                                         data));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                  _responseSettingBLL.SetResponse(ex));
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
        public IActionResult GetBankFinancialReport(GetBankFinancialReportReq request)
        {
            try
            {
                var data = _bankFinancialBLL.GetBankFinancialReport(JsonConvert.SerializeObject(request.SiteIds),
                                                                  JsonConvert.SerializeObject(request.CurrencyIds),
                                                                  request.StartDate,
                                                                  request.EndDate);
                if (data.Count() == 0)
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<BankFinancialDataModel>>(Service.Model.enums.ErrorMessageCodeEnum.NoData,
                                                                         data));
                }
                else
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<BankFinancialDataModel>>(Service.Model.enums.ErrorMessageCodeEnum.Success,
                                                                         data));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                  _responseSettingBLL.SetResponse(ex));
            }
        }

        [HttpPost]
        public IActionResult ExportBankFinancialReport(int SiteId,
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


        #endregion



    }
}
