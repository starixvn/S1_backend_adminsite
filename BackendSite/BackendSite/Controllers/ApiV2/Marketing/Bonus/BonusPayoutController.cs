using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using Common = BackendSite.Service.Library.Common;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusPayoutController : ControllerBase
    {
        #region Fields
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly BonusPayoutBLL _bonusPayoutBLL;
        private readonly Common _common;
        
        #endregion

        #region Constructors
        public BonusPayoutController(IServiceProvider provider, IConfiguration configuration)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _bonusApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").BonusService;

            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _bonusPayoutBLL = provider.GetRequiredService<BonusPayoutBLL>();
            _common = provider.GetRequiredService<Common>();
        }
        #endregion

        #region API Model
        public class BonusPayoutReq
        {
            public int CustId { get; set; }
            public decimal Amount { get; set; }
            public string Memo { set; get; }
            public string BonusCode { get; set; }
            public int RollOver { get; set; }
            public PromotionTypeEnum PromotionType { get; set; }
            public string DepositId { get; set; }
        }

        public class CheckTierRedepositBonusReq
        {
            public int CustId { get; set; }
            public string BonusCode { get; set; }
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string DepositId { get; set; }
        }


        public class CheckTierRedepositBonusRep
        {
            public bool IsSuccess { get; set; }
            public string DepositId { get; set; }
            public decimal ActualDeposit { get; set; }
            public decimal LockAmount { get; set; }
            public decimal BonusAmount { get; set; }
            public decimal RolloverAmount { get; set; }
            public int RollOver { get; set; }
            public int WinOver { get; set; }
        }

        public class SetBonusPayoutBatchReq
        {
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public PromotionTypeEnum PromotionType { get; set; }
            public IFormFile ExcelAdd { get; set; }
            public decimal Amount { get; set; }
        }
        #endregion

        /// <summary>
        /// BonusPayout/SetBonusPayout 後台手動領取Bonus
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance  > BonusPayout > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 33, 5 })]
        [HttpPost]
        public async Task<ApiResult> SetBonusPayout(BonusPayoutReq request)
        {
            var apiResult = new ApiResult();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                if (request.Amount <= 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Amount can't be zero or less than zero";
                    return apiResult;
                }

                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);

                var detailInfo = new
                {
                    UserName = custInfo.UserName,
                    BonusCode = request.BonusCode,
                    Memo = request.Memo,
                    Amount = request.Amount,
                    PromotionType = request.PromotionType.ToString(),
                    request.DepositId,
                };

                var logResult =
                    _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                    {
                        Operator = authData.UserName,
                        SiteId = custInfo.SiteId,
                        CurrencyId = custInfo.CurrencyId,
                        ActionType = ActionType.Create,
                        MenuEnum = AdminAdjustmentLogMenuEnum.IssueBonusManagement,
                        BeforeDetailInfo = null,
                        AfterDetailInfo = JsonConvert.SerializeObject(detailInfo),
                        RefTransId = string.Empty
                    });

                if (!logResult)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Insert log error.";
                    return apiResult;
                }

                var setAdjustBonusReq = new SetAdjustBonusReq()
                {
                    CustId = custInfo.CustId,
                    BonusCode = request.BonusCode,
                    Memo = request.Memo,
                    Operator = authData.UserName,
                    Amount = request.Amount,
                    PromotionType = (int)request.PromotionType,
                    DepositId = request.DepositId,
                };
                var apiOnlineResult =
                    await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = $"Success.";
                }
                else
                {
                    apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                    apiResult.Message = apiOnlineResult.Message;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
            }
            return apiResult;
        }

        /// <summary>
        /// BonusPayout/CheckTierRedepositBonus 檢查 Tier Redeposit Bonus
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance  > BonusPayout > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 33, 5 })]
        [HttpPost]
        public ApiResult<CheckTierRedepositBonusRep> CheckTierRedepositBonus(CheckTierRedepositBonusReq request)
        {
            return new ApiResult<CheckTierRedepositBonusRep>()
            {
                Data = _bonusPayoutBLL.CheckTierRedepositBonus(request),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 33, 10 })]
        [HttpPost]
        public async Task<List<ApiResult>> SetBonusPayoutBatch([FromForm] SetBonusPayoutBatchReq request)
        {
            var apiResult = new List<ApiResult>();
            var authData = _authorizeBll.GetAuthData();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                if (request.ExcelAdd.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase)
                    || request.ExcelAdd.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase)
                    || request.ExcelAdd.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(request.ExcelAdd.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    var isLastRow = false;
                    var rowId = 2;   // 因為有標題列，所以從第2列開始讀起

                    if (request.PromotionType == PromotionTypeEnum.RetentionFreeBet)
                    {
                        do  // 讀取資料，直到讀到空白列為止
                        {
                            var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text) ? null : sheet.Cells[rowId, 1].Text.Trim();

                            var sabaId = string.IsNullOrEmpty(sheet.Cells[rowId, 2].Text) ? null : sheet.Cells[rowId, 2].Text.Trim();
                            var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);
                            if (custUsername == null && sabaId == null)
                            {
                                isLastRow = true;
                            }
                            else
                            {
                                var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, request.SiteId);
                                if (custInfosDetail == null)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.CustNotFound, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                                }
                                else
                                {
                                    var detailInfo = new
                                    {
                                        UserName = custUsername,
                                        SabaId = sabaId,
                                        BonusCode = request.BonusCode,
                                        PromotionType = request.PromotionType.ToString()
                                    };
                                    if (!AddLog(custInfosDetail.CurrencyId, JsonConvert.SerializeObject(detailInfo)))
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = $"Insert log error. Username: {custUsername}. SabaId: {sabaId}." });
                                    }
                                    else
                                    {
                                        var setAdjustBonusReq = new SetAdjustBonusReq()
                                        {
                                            CustId = custInfosDetail.CustId,
                                            BonusCode = request.BonusCode,
                                            Memo = string.Empty,
                                            Operator = authData.UserName,
                                            Amount = request.Amount,
                                            PromotionType = (int)request.PromotionType,
                                            DepositId = null,
                                        };
                                        var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                        else
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = apiOnlineResult.ErrorCode, Message = $"{apiOnlineResult.Message}. Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                    }
                                }
                            }
                            rowId += 1;
                        } while (!isLastRow);
                    }
                    else if (request.PromotionType == PromotionTypeEnum.SpecialBonus)
                    {
                        do  // 讀取資料，直到讀到空白列為止
                        {
                            var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text) ? null : sheet.Cells[rowId, 1].Text.Trim();

                            var sabaId = string.IsNullOrEmpty(sheet.Cells[rowId, 2].Text) ? null : sheet.Cells[rowId, 2].Text.Trim();
                            var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);
                            decimal.TryParse(sheet.Cells[rowId, 3].Text.Trim(), out decimal amount);
                            if (custUsername == null && sabaId == null)
                            {
                                isLastRow = true;
                            }
                            else
                            {
                                var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, request.SiteId);
                                if (custInfosDetail == null)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.CustNotFound, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                                }
                                else
                                {
                                    var detailInfo = new
                                    {
                                        UserName = custUsername,
                                        SabaId = sabaId,
                                        BonusCode = request.BonusCode,
                                        PromotionType = request.PromotionType.ToString(),
                                        Amount = amount,
                                    };
                                    if (!AddLog(custInfosDetail.CurrencyId, JsonConvert.SerializeObject(detailInfo)))
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = $"Insert log error. Username: {custUsername}. SabaId: {sabaId}." });
                                    }
                                    else
                                    {
                                        var setAdjustBonusReq = new SetAdjustBonusReq()
                                        {
                                            CustId = custInfosDetail.CustId,
                                            BonusCode = request.BonusCode,
                                            Memo = string.Empty,
                                            Operator = authData.UserName,
                                            Amount = amount,
                                            PromotionType = (int)request.PromotionType,
                                            DepositId = null,
                                        };
                                        var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                        else
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = apiOnlineResult.ErrorCode, Message = $"Fail. {apiOnlineResult.Message} Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                    }
                                }
                            }
                            rowId += 1;
                        } while (!isLastRow);
                    }
                    else //Redeposit Bonus
                    {
                        do  // 讀取資料，直到讀到空白列為止
                        {
                            var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text) ? null : sheet.Cells[rowId, 1].Text.Trim();
                            var sabaId = string.IsNullOrEmpty(sheet.Cells[rowId, 2].Text) ? null : sheet.Cells[rowId, 2].Text.Trim();
                            var custId = string.IsNullOrEmpty(sabaId) ? null : _common.TpIdToCustId(sabaId);
                            var depositId = string.IsNullOrEmpty(sheet.Cells[rowId, 3].Text) ? null : sheet.Cells[rowId, 3].Text.Trim();

                            if (custUsername == null && sabaId == null)
                            {
                                isLastRow = true;
                            }
                            else
                            {
                                var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, request.SiteId);
                                if (custInfosDetail == null)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.CustNotFound, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                                }
                                else
                                {
                                    var detailInfo = new
                                    {
                                        UserName = custUsername,
                                        SabaId = sabaId,
                                        BonusCode = request.BonusCode,
                                        PromotionType = request.PromotionType.ToString(),
                                        DepositId = depositId
                                    };
                                    if (!AddLog(custInfosDetail.CurrencyId, JsonConvert.SerializeObject(detailInfo)))
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = $"Insert log error. Username: {custUsername}. SabaId: {sabaId}." });
                                    }
                                    else
                                    {
                                        var setAdjustBonusReq = new SetAdjustBonusReq()
                                        {
                                            CustId = custInfosDetail.CustId,
                                            BonusCode = request.BonusCode,
                                            Memo = string.Empty,
                                            Operator = authData.UserName,
                                            PromotionType = (int)request.PromotionType,
                                            DepositId = depositId,
                                        };
                                        var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                                        if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                        else
                                        {
                                            apiResult.Add(new ApiResult() { ErrorCode = apiOnlineResult.ErrorCode, Message = $"{apiOnlineResult.Message}. Username: {custUsername}. SabaId: {sabaId}." });
                                        }
                                    }
                                }
                            }
                            rowId += 1;
                        } while (!isLastRow);
                    }
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "File Type Error" });
                    return apiResult;
                }
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = (int)ErrorCodeEnum.SystemException, Message = ex.ToString() });
                return apiResult;
            }

            #region CreateAdminAdjustmentInfoLog
            bool AddLog(int currencyId, string afterDetailInfo)
            {
                return _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = authData.UserName,
                    SiteId = request.SiteId,
                    CurrencyId = currencyId,
                    ActionType = ActionType.Create,
                    MenuEnum = AdminAdjustmentLogMenuEnum.IssueBonusManagement,
                    BeforeDetailInfo = null,
                    AfterDetailInfo = afterDetailInfo,
                    RefTransId = string.Empty
                });
            }
            #endregion
        }
    }
}
