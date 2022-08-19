using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;


namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        private readonly ICustomerBLL _customerBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly CustVerifyBLL _custVerifyBll;

        public CustomerManageController(ICustomerBLL customerBll, CustInfoBLL custInfoBll, CustVerifyBLL custVerifyBll)
        {
            _customerBll = customerBll;
            _custInfoBll = custInfoBll;
            _custVerifyBll = custVerifyBll;
        }

        /// <summary>
        /// CustomerManage/GetCustomerBasicInfo 取得客戶基本資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public ApiResult<CustomerDto> GetBasicInfo(GetCustomerBasicInfoRequest request)
        {
            var data = _customerBll.GetCustomerBasicInfo(request.CustId);

            var apiResult = new ApiResult<CustomerDto>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
        
        #region Finance > Balance Adjustment

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
            var status = (CustomerStatusEnum) custInfo.Status;
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
                        ErrorCode = (int) ErrorCodeEnum.Success
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
            var status = (CustomerStatusEnum) custInfo.Status;
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
                            TpWalletList = custTPWallet.Where(x=>x.WalletType == (int)TpListWalletTypeEnum.FundTransfer)
                        },
                        ErrorCode = (int) ErrorCodeEnum.Success
                    },
                    
                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),
                    
                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }

        #endregion
    }
}
