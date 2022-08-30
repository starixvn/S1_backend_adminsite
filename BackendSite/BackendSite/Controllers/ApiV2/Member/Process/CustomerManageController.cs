using System.Text.RegularExpressions;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL.Interface;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        private readonly PermissionBLL _permissionBll;
        private readonly ICustomerBLL _customerBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly CustVerifyBLL _custVerifyBll;
        private readonly CustPhotoIdBLL _custPhotoIdBll;
        
        public CustomerManageController(ICustomerBLL customerBll, CustInfoBLL custInfoBll, CustVerifyBLL custVerifyBll, CustPhotoIdBLL custPhotoIdBll, PermissionBLL permissionBll)
        {
            _customerBll = customerBll;
            _custInfoBll = custInfoBll;
            _custVerifyBll = custVerifyBll;
            _custPhotoIdBll = custPhotoIdBll;
            _permissionBll = permissionBll;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 61 })]
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
        #endregion
        
        #region CSD > Customer Administration > CustInfo

        /// <summary>
        /// CustomerManage/GetCustInfoList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;
            var tagStartDate = request.TagStartDate != new DateTimeOffset() ? request.TagStartDate?.DateTime : null;
            var tagEndDate = request.TagEndDate != new DateTimeOffset() ? request.TagEndDate?.DateTime : null;
            var statusStartDate = request.StatusStartDate != new DateTimeOffset() ? request.StatusStartDate?.DateTime : null;
            var statusEndDate = request.StatusEndDate != new DateTimeOffset() ? request.StatusEndDate?.DateTime : null;

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            var pageNumber = request.PageNumber;
            var data = _custInfoBll.GetCustInfoList(request.SiteIds,
                request.BirDate,
                request.BirMonth,
                request.BirYear,
                request.UserName?.Trim(),
                request.AccountName?.Trim(),
                request.LoginIp?.Trim(),
                request.TransId?.Trim(),
                request.TpUserName?.Trim(),
                request.Email?.Trim(),
                request.PhoneNo?.Trim(),
                startDate,
                endDate,
                tagStartDate,
                tagEndDate,
                statusStartDate,
                statusEndDate,
                request.PageSize,
                request.AffCode,
                request.CurrencyIds,
                request.BankAccountNumber?.Trim(),
                request.Statuses,
                request.SelectTag,
                ref pageNumber,
                out var totalPages,
                out var rowCount);

            var custInfoDetails = data.ToList();
            if (!custInfoDetails.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            return new ApiListResult<CustInfoDetail>
            {
                Data = custInfoDetails,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// CustomerManage/GetCustInfoDetail 取得客戶資料明細
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustInfoDetail(CustIdRequest request)
        {

            var custInfoDetail = _custInfoBll.GetCustInfoDetail(request.CustId, null);

            if (custInfoDetail != null)
            {
                if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_readDetail"))
                {
                    custInfoDetail.Email = MaskEmail(custInfoDetail.Email);
                    custInfoDetail.PhoneNo = MaskPhone(custInfoDetail.PhoneNo);
                    custInfoDetail.CustLineProfile.userId = MaskLineId(custInfoDetail.CustLineProfile.userId);
                }

                return new ApiResult<CustInfoDetail>
                {
                    Data = custInfoDetail,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };

            }
            else
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
        }
        
        #endregion
        
        /// <summary>
        /// 電話遮罩
        /// </summary>
        /// <param name="phone">電話</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            var pattern = @"(?<=[\w]{3})[\w-\._\+%]*(?=[\w]{3})";
            return Regex.Replace(phone, pattern, m => new string('*', m.Length));
        }
        /// <summary>
        /// EMail遮罩
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            return Regex.Replace(email, pattern, m => new string('*', m.Length));
        }
        /// <summary>
        /// LineUserId遮罩
        /// </summary>
        /// <param name="data">Email</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskLineId(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            return Regex.Replace(data, pattern, m => new string('*', m.Length));
        }
    }
}

