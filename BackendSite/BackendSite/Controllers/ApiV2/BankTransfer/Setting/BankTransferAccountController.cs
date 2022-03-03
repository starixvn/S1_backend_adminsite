using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.BankTransfer.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankTransferAccountController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBll;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public BankTransferAccountController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
            public int Id { get; set; }
        }

        public class ExportRequest
        {
            public int SiteId { get; set; }
        }

        public class CreateTransferAccountReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        #endregion

        #region Action

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>> GetList(GetListRequest req)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetTransferAccountInfoListForDisplay(false, req.SiteId, req.Id);
                foreach (var data in apiResult.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                    data.UpdateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var ExportData = _bankFinancialBll.GetTransferAccountInfoListForExport(false, request.SiteId, 0);
            foreach (var data in ExportData.ToArray())
            {
                data.Purpose = GetPurpose(data.Purpose);
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.Export(ExportData);
            return _excelBll.ExcelFile(stream, "TransferAccountInfoList");
        }

        //[HttpPost]
        //public ApiResult CreateTransferAccount(CreateTransferAccountReq req)
        //{
        //    var apiResult = new ApiResult<BankFinancialTransferAccountDetailModel>();

        //    try
        //    {
        //        var authData = _authorizeBll.GetAuthData();
        //        var dateTime =  new DateTimeOffset(DateTime.Now, TimeSpan.Zero);
        //        apiResult.Data = new BankFinancialTransferAccountDetailModel()
        //        {
        //            SiteId = req.SiteId,
        //            CurrencyId = req.CurrencyId,
        //            InitialAmount = 0,
        //            MaxBalanceAlertAmount = 0,
        //            Status = TransferAccountStatusEnum.Open,
        //            CreateTime = dateTime,
        //            Creator = authData.UserId,
        //            CreatorName = authData.UserName,
        //            UpdateTime = dateTime,
        //            Updator = authData.UserId,
        //            UpdatorName = authData.UserName,
        //        };

        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResult.RawMessage = ex.Message;
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.Message = "something went wrong.";
        //    }

        //    return apiResult;
        //}

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        [HttpPost]
        public ApiResult<BankFinancialTransferAccountDetailModel> GetById(GetListRequest req)
        {
            var apiResult = new ApiResult<BankFinancialTransferAccountDetailModel>();

            try
            {
                var result = _bankFinancialBll.GetTransferAccountInfoListForDisplay(true, req.SiteId, req.Id);

                foreach (var data in result.ToArray())
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                }

                if (result.Count() == 1)
                {
                    var data = result.FirstOrDefault();

                    var authData = _authorizeBll.GetAuthData();
                    var dateTime = new DateTimeOffset(DateTime.Now.ToUniversalTime(), TimeSpan.Zero);

                    data.Updator = authData.UserId;
                    data.UpdatorName = authData.UserName;
                    data.UpdateTime = dateTime;

                    apiResult.Data = data;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        [HttpPost]
        public ApiResult Set(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {

            var apiResult = new ApiResult<int>();

            try
            {
                var (status, count, message) = _bankFinancialBll.SetTransferAccount(bankFinancialTransferAccount);

                if (status)
                {
                    apiResult.Data = count;
                    apiResult.Message = message;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        #endregion

        #region Private Methods

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