using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
    public class TransferAccountManageController : Controller
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBLL;
        private readonly BankFinancialBLL _bankFinancialBLL;
        private readonly BankDetailBLL _bankDetailBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public TransferAccountManageController(IConfiguration configuration,
                AuthorizeBLL authorizeBLL,
                BankFinancialBLL bankFinancialBLL,
                BankDetailBLL bankDetailBLL,
                ExcelBLL excelBLL)
        {
            _authorizeBLL = authorizeBLL;
            _bankFinancialBLL = bankFinancialBLL;
            _bankDetailBLL = bankDetailBLL;
            _excelBLL = excelBLL;
        }

        #endregion

        public class TitleModel
        {
            public string Name { get; set; }

            public int SortId { get; set; }
        }


        #region API Model

        public class GetTransferAccountInfoReq
        {
            public int SiteId { get; set; }

            public int Id { get; set; }
        }

        public class CreateTransferAccountReq
        {
            public int SiteId { get; set; }

            public int CurrencyId { get; set; }
        }

        #endregion

        #region Action

        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManages" })]
        public ApiResult GetTransferAccountInfoList(GetTransferAccountInfoReq req)
        {

            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBLL.GetTransferAccountInfoList(false, req.SiteId, req.Id);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManages" })]
        [HttpPost]
        public ApiResult CreateTransferAccount(CreateTransferAccountReq req)
        {

            var apiResult = new ApiResult<BankFinancialTransferAccountDetailModel>();

            try
            {
                var authData = _authorizeBLL.GetAuthData();
                string dateTime = GetTimeZoneDateTime(req.SiteId);
                apiResult.Data = new BankFinancialTransferAccountDetailModel()
                {
                    SiteId = req.SiteId,
                    CurrencyId = req.CurrencyId,

                    InitialAmount = 0,
                    MaxBalanceAlertAmount = 0,
                    Status = TransferAccountStatusEnum.Open,
                    CreateTime = dateTime,
                    Creator = authData.UserId,
                    CreatorName = authData.UserName,
                    UpdateTime = dateTime,
                    Updator = authData.UserId,
                    UpdatorName = authData.UserName,
                };

                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManages" })]
        [HttpPost]
        public ApiResult<BankFinancialTransferAccountDetailModel> GetTransferAccountData(GetTransferAccountInfoReq req)
        {

            var apiResult = new ApiResult<BankFinancialTransferAccountDetailModel>();

            try
            {
                var result = _bankFinancialBLL.GetTransferAccountInfoList(true, req.SiteId, req.Id);
                
                if ( result.Count() == 1)
                {
                    var data = result.FirstOrDefault();

                    var authData = _authorizeBLL.GetAuthData();
                    string dateTime = GetTimeZoneDateTime(req.SiteId);
                    
                    data.Updator = authData.UserId;
                    data.UpdatorName = authData.UserName;
                    data.UpdateTime = dateTime;

                    apiResult.Data = data;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                    apiResult.Message = "The Transfer Account Detail Is Not Exist.";
                }
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManages" })]
        [HttpPost]
        public ApiResult SetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {

            var apiResult = new ApiResult<int>();

            try
            {
                var result = _bankFinancialBLL.SetTransferAccount(bankFinancialTransferAccount);

                if (result.Status)
                {
                    apiResult.Data = result.Count;
                    apiResult.RawMessage = result.Message;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.Data = result.Count;
                    apiResult.RawMessage = result.Message;
                    apiResult.ErrorCode = 500;
                }

            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportTransferAccountInfoList(int SiteId)
        {
            var ExportData = _bankFinancialBLL.GetTransferAccountInfoList(false, SiteId, 0);
            var stream = _excelBLL.Export(ExportData);
            return File(stream, 
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       $"TransferAccountInfoList_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }


        #region Data For Set

        public ApiResult GetTransferAccountReason()
        {

            var apiResult = new ApiResult<IEnumerable<BankFinancialReasonModel>>();

            try
            {
                apiResult.Data = _bankFinancialBLL.GetReason("BankFinancial/TransferAccountManage");
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        public ApiResult GetSysBankList()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoSys>>();

            try
            {
                apiResult.Data = _bankDetailBLL.GetSysBankList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        public ApiResult GetPayList()
        {
            var apiResult = new ApiResult<IEnumerable<PayList>>();

            try
            {
                apiResult.Data = _bankFinancialBLL.GetPayList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        #endregion

        #endregion

        #region Private Methods

        private string GetTimeZoneDateTime(int siteId)
        {
            int timeZone = siteId == 1 ? 8 : 7;
            return new DateTimeOffset(DateTime.Now).ToOffset(new TimeSpan(timeZone, 0, 0)).DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #endregion
    }
}
