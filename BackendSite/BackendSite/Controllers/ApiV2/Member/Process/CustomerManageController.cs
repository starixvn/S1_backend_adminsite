using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly CustVerifyBLL _custVerifyBll;

        public CustomerManageController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _custVerifyBll = provider.GetRequiredService<CustVerifyBLL>();
        }

        #region Model 

        public class GetCustInfoWithBalanceResult
        {
            public int SiteId { get; set; }
            public string SiteName { get; set; }
            public int CustId { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public CustomerStatusEnum Status { get; set; }
            public decimal Balance { get; set; }
        }

        public class GetCustInfoWithBalanceRequest
        {
            public int SiteId { get; set; }
            public string UserName { get; set; }
            public string SabaId { get; set; }
        }

        public class GetCustInfoWithTpBalanceResult : GetCustInfoWithBalanceResult
        {
            public IEnumerable<TpWallet> TpWalletList { get; set; }
        }

        #endregion

        // todo 待確認權限
        [HttpPost]
        public async Task<ApiListResultCurrencyAmount<PromotionDto>> GetPromotionByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<PromotionDto>();
            try
            {
                var queryResult = await _custInfoBll.GetPromotionListByCustIdAndDateAsync(request);
                apiResult.Data = queryResult.PromotionList;
                apiResult.PageNumber = queryResult.PageNumber;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmount = queryResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// CustomerManage/GetCustInfoWithBalance 取得客戶資料及主錢包餘額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Search] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<GetCustInfoWithBalanceResult> GetCustInfoWithBalance(GetCustInfoWithBalanceRequest request)
        {
            var result =
                _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            var balance = _custInfoBll.GetBalance(custInfo.CustId);
            var status = (CustomerStatusEnum)custInfo.Status;
            return status switch
            {
                CustomerStatusEnum.Running
                    => new ApiResult<GetCustInfoWithBalanceResult>
                    {
                        Data = new GetCustInfoWithBalanceResult
                        {
                            SiteId = custInfo.SiteId,
                            SiteName = custInfo.SiteName,
                            CustId = custInfo.CustId,
                            CurrencyId = custInfo.CurrencyId,
                            UserName = custInfo.UserName,
                            Status = status,
                            Balance = balance
                        },
                        ErrorCode = (int)ErrorCodeEnum.Success
                    },

                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),

                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }

        /// <summary>
        /// CustomerManage/GetCustInfoWithTpBalance 取得客戶資料、主錢包餘額及三方錢包餘額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Search] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<GetCustInfoWithTpBalanceResult>> GetCustInfoWithTpBalanceAsync(GetCustInfoWithBalanceRequest request)
        {
            var result =
                _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            var balance = await _custInfoBll.GetBalanceAsync(custInfo.CustId);
            var custTPWallet = await _custInfoBll.GetCustTPWalletAsync(custInfo.CustId);
            var status = (CustomerStatusEnum)custInfo.Status;
            return status switch
            {
                CustomerStatusEnum.Running
                    => new ApiResult<GetCustInfoWithTpBalanceResult>
                    {
                        Data = new GetCustInfoWithTpBalanceResult
                        {
                            SiteId = custInfo.SiteId,
                            SiteName = custInfo.SiteName,
                            CustId = custInfo.CustId,
                            CurrencyId = custInfo.CurrencyId,
                            UserName = custInfo.UserName,
                            Status = status,
                            Balance = balance,
                            TpWalletList = custTPWallet.Where(x => x.WalletType == (int)TpListWalletTypeEnum.FundTransfer)
                        },
                        ErrorCode = (int)ErrorCodeEnum.Success
                    },

                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),

                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }
    }
}

