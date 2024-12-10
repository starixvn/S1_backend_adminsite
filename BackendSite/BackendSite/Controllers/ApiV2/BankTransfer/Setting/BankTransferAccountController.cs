using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        private readonly Cryptor _cryptor;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructors

        public BankTransferAccountController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _cryptor = provider.GetRequiredService<Cryptor>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region Action

        /// <summary>
        /// BankTransferAccount/GetList 取得銀行轉帳戶口資料清單
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialTransferAccountDisplayModel>> GetList(GetTransferAccountInfoListRequest req)
        {
            var list =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(req));

            foreach (var data in list)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            var result =
                list.Select(x => new BankFinancialTransferAccountDisplayModel(x));

            return new ApiResult<IEnumerable<BankFinancialTransferAccountDisplayModel>>
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
            var datas =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(request));

            var result = datas.Select(x =>
                new BankFinancialTransferAccountExportModel
                {
                    Id = x.Id,
                    SiteName = x.SiteName,
                    Currency = x.IsoCode,
                    Channel = x.Channel.ToString(),
                    Agent = x.Agent,
                    PayName = x.PayName,
                    CryptoCurrency = x.CryptoCurrencyId?.ToString(),
                    CryptoNetwork = x.Network,
                    Purpose = string.Join(", ", x.Purposes.Select(x => x.ToString())),
                    BankCode = x.BankCode,
                    AccountNo = x.AccountNo,
                    AccountName = x.AccountName,
                    DisplayName = x.DisplayName,
                    FrontendDisplayName = x.FrontendDisplayName,
                    Status = x.Status.ToString(),
                    InitialAmount = x.InitialAmount,
                    MaxBalanceAlertAmount = x.MaxBalanceAlertAmount,
                    Remark = x.Remark,
                    BankLoginUrl = x.BankLoginUrl,
                    /*BankLoginInfo = string.Join("|", _bankFinancialBll.CryptBankLoginInfo(x.BankLoginInfo, false)
                        .Select(y => $"acc:{y.Account}, pw:{y.Password}")),*/
                    TransferPin = _cryptor.Decrypt(x.TransferPin),
                    LoginPin = _cryptor.Decrypt(x.LoginPin),
                    MobilePin = _cryptor.Decrypt(x.MobilePin),
                    CorporateID = _cryptor.Decrypt(x.CorporateID),
                    CreateTime = new DateTimeOffset(x.CreateTime.DateTime, TimeSpan.Zero),
                    Creator = x.CreatorName,
                    UpdateTime = new DateTimeOffset(x.UpdateTime.DateTime, TimeSpan.Zero),
                    Updator = x.UpdatorName
                });

            var stream = _excelBll.ExportV2(result);
            return _excelBll.ExcelFile(stream, "TransferAccountInfoList");
        }

        /// <summary>
        /// BankTransferAccount/GetById 取得指定銀行轉帳戶口資料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<BankFinancialTransferAccountDetailModel> GetById(GetTransferAccountInfoByIdRequest req)
        {
            var result =
                _bankFinancialBll.GetTransferAccountInfoList(new GetTransferAccountInfoListParams(req.Id));

            if (!result.Any())
            {
                throw new AdminException(ErrorCodeEnum.Fail, "The Transfer Account Detail Is Not Exist.");
            }

            var data = result.FirstOrDefault();
            data.BankLoginInfo = _bankFinancialBll.CryptBankLoginInfo(data.BankLoginInfo, false);
            data.TransferPin = _cryptor.Decrypt(data.TransferPin);
            data.LoginPin = _cryptor.Decrypt(data.LoginPin);
            data.MobilePin = _cryptor.Decrypt(data.MobilePin);
            data.CorporateID = _cryptor.Decrypt(data.CorporateID);

            return new ApiResult<BankFinancialTransferAccountDetailModel>
            {
                Data = data,
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
            var siteName = _serverInfoBll.GetServerInfo(req.SiteId).SiteName;
            var (status, message) = _bankFinancialBll.SetTransferAccount(new BankFinancialTransferAccountModel
            {
                Id = req.Id,
                SiteId = req.SiteId,
                SiteName = siteName,
                CurrencyId = req.CurrencyId,
                AgentId = req.AgentId,
                Channel = req.Channel,
                Purpose = req.Purposes,
                Reason = req.Reason,
                Remark = req.Remark,
                Status = req.Status,
                AccountName = req.AccountName,
                AccountNo = req.AccountNo,
                AccountCode = req.AccountCode,
                DisplayName = req.DisplayName,
                FrontendDisplayName = req.FrontendDisplayName,
                InitialAmount = req.InitialAmount,
                InitialTime = req.InitialTime,
                MaxBalanceAlertAmount = req.MaxBalanceAlertAmount,
                PayId = req.PayId,
                SysId = req.SysId,
                Updator = authData.UserId,
                BankLoginUrl = req.BankLoginUrl,
                BankLoginInfo = req.BankLoginInfo,
                TransferPin = req.TransferPin,
                LoginPin = req.LoginPin,
                MobilePin = req.MobilePin,
                CorporateID = req.CorporateID,
                CryptoCurrencyId = req.CryptoCurrencyId,
                Network = req.Network
            }, authData.UserName);

            return new ApiResult
            {
                Message = message,
                ErrorCode = (int)(status ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult GetTransferAccountReason()
        {
            var result = _bankFinancialBll.GetReason("BankFinancial/TransferAccountManage");

            return new ApiResult<IEnumerable<BankFinancialReasonModel>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult GetPayList()
        {
            var apiResult = new ApiResult<IEnumerable<PayList>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetPayList()
                    .Where(x => x.Status == (int)GeneralStatusEnum.Enable);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult GetAgentList(GetAgentListRequest req)
        {
            var result = _bankFinancialBll.GetTransferAccountAgentList(req.SiteId);

            return new ApiResult<IEnumerable<string>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion
    }
}