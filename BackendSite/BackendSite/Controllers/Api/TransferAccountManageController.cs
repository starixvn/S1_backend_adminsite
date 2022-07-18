using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
    [ApiExplorerSettings(IgnoreApi = true)]
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
                apiResult.Data = _bankFinancialBLL.GetTransferAccountInfoListForDisplay(false, req.SiteId, req.Id);
               
                foreach (var data in apiResult.Data.ToArray())
                {
                    data.CreateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.CreateTime);
                    data.UpdateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.UpdateTime);
                }
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
                var dateTime = GetTimeZoneDateTimeBySiteId(req.SiteId);
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
                var result = _bankFinancialBLL.GetTransferAccountInfoListForDisplay(true, req.SiteId, req.Id);

                foreach (var data in result.ToArray())
                {
                    data.CreateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.CreateTime);
                    data.UpdateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.UpdateTime);
                }

                if (result.Count() == 1)
                {
                    var data = result.FirstOrDefault();
                    var authData = _authorizeBLL.GetAuthData();

                    data.Updator = authData.UserId;
                    data.UpdatorName = authData.UserName;
                    data.UpdateTime = GetTimeZoneDateTimeBySiteId(req.SiteId);
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
                var (status, count, message) = _bankFinancialBLL.SetTransferAccount(bankFinancialTransferAccount,
                    _authorizeBLL.GetAuthData().UserName);

                if (status)
                {
                    apiResult.Data = count;
                    apiResult.Message = message;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.Data = count;
                    apiResult.Message = message;
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
            var ExportData = _bankFinancialBLL.GetTransferAccountInfoListForExport(false, SiteId, 0);
            foreach (var data in ExportData.ToArray())
            {
                data.Purpose = GetPurpose(data.Purpose);
                data.CreateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.CreateTime);
                data.UpdateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.UpdateTime);
            }
            
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

        private DateTimeOffset GetTimeZoneDateTimeBySiteId(int siteId)
        {
            var timeZone = (siteId == 2 || siteId == 3) ? 7 : 8;
            return new DateTimeOffset(DateTime.Now).ToOffset(new TimeSpan(timeZone, 0, 0));
        }

        private DateTimeOffset GetTimeZoneDateTimeByDateString(int siteId, DateTimeOffset datetime)
        {
            var timeZone = (siteId == 2 || siteId == 3) ? 7 : 8;
            return new DateTimeOffset(datetime.DateTime, new TimeSpan(timeZone,0,0)).AddHours(timeZone);
        }
        
        private string GetPurpose(string purpose)
        {
            var result = string.Empty;

            IEnumerable<int> purposeList = purpose.Split(',').Select(int.Parse).ToList();

            foreach (var i in purposeList)
            {
                switch (i)
                {
                    case 1: result += ",Deposit"; break;
                    case 2: result += ",Withdrawal"; break;
                    case 3: result += ",Transfer"; break;
                }

            }
            return result.Substring(1);
        }
        
        #endregion
    }
}
