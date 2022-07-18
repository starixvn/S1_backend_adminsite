using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.BankTransfer.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankTransferAccountController : ControllerBase
    {
        #region Fields

        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public BankTransferAccountController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Action

        /// <summary>
        /// BankTransferAccount/GetList 取得銀行轉帳戶口資料清單
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>> GetList(GetTransferAccountInfoListRequest req)
        {
            var result =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(req));

            foreach (var data in result)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }
            
            return new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// BankTransferAccount/Export 匯出銀行轉帳戶口資料清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Export(GetTransferAccountInfoListRequest request)
        {
            var result =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(request));

            foreach (var data in result)
            {
                data.Purpose = string.Join(", ", data.Purposes.Select(x => x.ToString()));
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }
            
            var stream = _excelBll.Export(result);
            return _excelBll.ExcelFile(stream, "TransferAccountInfoList");
        }
        
        /// <summary>
        /// BankTransferAccount/GetById 取得指定銀行轉帳戶口資料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        [HttpPost]
        public ApiResult<BankFinancialTransferAccountDetailModel> GetById(GetTransferAccountInfoListByIdRequest req)
        {
            var result =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(req.Id));

            if (!result.Any())
            {
                throw new AdminException(ErrorCodeEnum.Fail, "The Transfer Account Detail Is Not Exist.");
            }

            return new ApiResult<BankFinancialTransferAccountDetailModel>
            {
                Data = result.FirstOrDefault(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

        }

        /// <summary>
        /// BankTransferAccount/Set 新增/更新銀行轉帳戶口資料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Set(SetTransferAccountInfoRequest req)
        {
            var authData = _authorizeBll.GetAuthData();

            var (status, count, message) = _bankFinancialBll.SetTransferAccount(new BankFinancialTransferAccountModel
            {
                Id = req.Id,
                SiteId = req.SiteId,
                SiteName = ((SiteEnum)req.SiteId).GetDescription(),
                CurrencyId = req.CurrencyId,
                Agent = req.Agent,
                Channel = req.Channel,
                Purpose = string.Join(",", req.Purposes.Select(x=> (int)x)),
                Reason = req.Reason,
                Remark = req.Remark,
                Status = req.Status,
                AccountName = req.AccountName,
                AccountNo = req.AccountNo,
                DisplayName = req.DisplayName,
                InitialAmount = req.InitialAmount,
                MaxBalanceAlertAmount = req.MaxBalanceAlertAmount,
                PayId = req.PayId,
                SysId = req.SysId,
                Updator = authData.UserId
            }, authData.UserName);

            if (status)
            {
                return new ApiResult
                {
                    Message = message,
                    ErrorCode = (int) ErrorCodeEnum.Success
                };
            }
            
            throw new AdminException(ErrorCodeEnum.Fail, message);
        }
        
        public ApiResult GetTransferAccountReason()
        {
            var result = _bankFinancialBll.GetReason("BankFinancial/TransferAccountManage");
            
            return new ApiResult<IEnumerable<BankFinancialReasonModel>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion
    }
}