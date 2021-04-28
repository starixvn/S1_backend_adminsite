using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
    public class TransferManageController : Controller
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBLL;
        private readonly BankFinancialBLL _bankFinancialBLL;
        private readonly BankDetailBLL _bankDetailBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public TransferManageController(IConfiguration configuration,
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

        public class GetTransferInfoReq
        {
            public TransferTypeEnum? TransferType { get; set; }

            public string TransFromDate { get; set; }

            public string TransToDate { get; set; }

            public string FromAccountNo { get; set; }

            public string FromAccountDisplayName { get; set; }

            public string ToAccountNo { get; set; }

            public string ToAccountDisplayName { get; set; }

            public int? Reason { get; set; }

            public string TransferId { get; set; }
        }

        public class CreateTransferInfoReq
        {
            public int SiteId { get; set; }
        }

        #endregion

        #region Action

        public ApiResult GetTransferInfoList(GetTransferInfoReq req)
        {

            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferInfoDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBLL.GetTransferInfoList(false,
                                                                       req.TransferType,
                                                                       req.TransFromDate,
                                                                       req.TransToDate,
                                                                       req.FromAccountNo,
                                                                       req.FromAccountDisplayName,
                                                                       req.ToAccountNo,
                                                                       req.ToAccountDisplayName,
                                                                       req.Reason,
                                                                       req.TransferId);
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

        [HttpPost]
        public ApiResult CreateTransferInfo()
        {

            var apiResult = new ApiResult<BankFinancialTransferInfoDetailModel>();
            try
            {
                var authData = _authorizeBLL.GetAuthData();
                string dateTime = GetTimeZoneDateTime(1);
                apiResult.Data = new BankFinancialTransferInfoDetailModel()
                {
                    TransDate = DateTime.Now.ToString("yyyy-MM-dd"),

                    FromAmount = 0,
                    FromFee = 0,
                    ToAmount = 0,
                    ToFee = 0,
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

        [HttpPost]
        public ApiResult<BankFinancialTransferInfoDetailModel> GetTransferInfoData(GetTransferInfoReq req)
        {

            var apiResult = new ApiResult<BankFinancialTransferInfoDetailModel>();

            try
            {
                var result = _bankFinancialBLL.GetTransferInfoList(true,
                                                                   req.TransferType,
                                                                   req.TransFromDate,
                                                                   req.TransToDate,
                                                                   req.FromAccountNo,
                                                                   req.FromAccountDisplayName,
                                                                   req.ToAccountNo,
                                                                   req.ToAccountDisplayName,
                                                                   req.Reason,
                                                                   req.TransferId);

                if (result.Count() == 1)
                {
                    var data = result.FirstOrDefault();

                    var authData = _authorizeBLL.GetAuthData();
                    string dateTime = GetTimeZoneDateTime(1);

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

        [HttpPost]
        public ApiResult SetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail)
        {

            var apiResult = new ApiResult<int>();

            try
            {
                var result = _bankFinancialBLL.SetTransferInfo(bankFinancialTransferInfoDetail);

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
        public ApiResult DeleteTransferInfoData(GetTransferInfoReq req)
        {

            var apiResult = new ApiResult<int>();

            try
            {
                var result = _bankFinancialBLL.DeleteTransferInfo(req.TransferId);

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
        public IActionResult ExportTransferInfoList(GetTransferInfoReq req)
        {
            var ExportData = _bankFinancialBLL.GetTransferInfoList(false,
                                                                       req.TransferType,
                                                                       req.TransFromDate,
                                                                       req.TransToDate,
                                                                       req.FromAccountNo,
                                                                       req.FromAccountDisplayName,
                                                                       req.ToAccountNo,
                                                                       req.ToAccountDisplayName,
                                                                       req.Reason,
                                                                       req.TransferId);
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
                apiResult.Data = _bankFinancialBLL.GetReason("BankFinancial/TransferManage");
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
