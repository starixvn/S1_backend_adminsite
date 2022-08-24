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
        private readonly CustPhotoIdBLL _custPhotoIdBll;
        
        public CustomerManageController(ICustomerBLL customerBll, CustInfoBLL custInfoBll, CustVerifyBLL custVerifyBll, CustPhotoIdBLL custPhotoIdBll)
        {
            _customerBll = customerBll;
            _custInfoBll = custInfoBll;
            _custVerifyBll = custVerifyBll;
            _custPhotoIdBll = custPhotoIdBll;
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
        public async Task<ApiResult<CustomerBasicInfo>> GetBasicInfo(GetCustomerBasicInfoRequest request)
        {
            var data = await _customerBll.GetCustomerBasicInfo(request.CustId);

            var apiResult = new ApiResult<CustomerBasicInfo>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
        
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GetCustPhotoIdResponse>>> GetCustPhotoId(GetCustPhotoIdRequest request)
        {
            var custPhotoId = await _custPhotoIdBll.GetPhotoIdAsync(request.CustId);
            if (custPhotoId == null) throw new AdminException(ErrorCodeEnum.NoData);
            
            var apiResult = new ApiResult<IEnumerable<GetCustPhotoIdResponse>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = custPhotoId
            };
            return apiResult;

        }
        
        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> UploadCustPhotoId([FromForm]UploadCustPhotoIdRequest request)
        {
            var result = await _custPhotoIdBll.UploadPhotoIdAsync(request.CustId, request.Photo);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Upload Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Upload Fail.");

        }
        
        /// <summary>
        /// Ask Re-Upload
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> DeleteCustPhotoId(DeleteCustPhotoIdRequest request)
        {
            var result = await _custPhotoIdBll.DeleteCustPhotoId(request.CustId, request.Id);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Delete Fail.");
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
