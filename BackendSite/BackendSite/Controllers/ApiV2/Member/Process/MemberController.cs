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
    public class MemberController : ControllerBase
    {
        private readonly IMemberDashboardBLL _memberDashboardBll;
        private readonly ICustPhotoIdBLL _custPhotoIdBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;

        public MemberController(IMemberDashboardBLL memberDashboardBll, ICustPhotoIdBLL custPhotoIdBll, CustInfoBLL custInfoBll, ServerInfoBLL serverInfoBll)
        {
            _memberDashboardBll = memberDashboardBll;
            _custPhotoIdBll = custPhotoIdBll;
            _custInfoBll = custInfoBll;
            _serverInfoBll = serverInfoBll;
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

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 49 })]
        [HttpPost]
        public ApiResult CancelBonusByBonusId(CancelBonusRequest request)
        {
            var apiResult = new ApiResult();
            _custInfoBll.CancelBonusByBonusId(request.BonusId);
            const string menu = "CustManage/CustInfoList";
            var info = new Dictionary<string, dynamic>
            {
                { "MenuName", "Customer Info" },
                { "Action", "Cancel Bonus" }
            };
            _serverInfoBll.AdminOperateLog(null, request.CustId, menu, ActionType.Update, info, User.Identity.Name);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }
        
        /// <summary>
        /// Member/GetCustomerBasicInfo 取得客戶基本資料
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
            var data = await _memberDashboardBll.GetCustomerBasicInfo(request.CustId);

            var apiResult = new ApiResult<CustomerBasicInfo>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
        
        /// <summary>
        /// Member/GetCustPhotoId 取得客戶證件照列表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// Member/UploadCustPhotoId 上傳用戶證件照
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
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
        /// Member/DeleteCustPhotoId 刪除用戶證件照
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
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

        /// <summary>
        /// Member/SetFullName 修改用戶真實姓名
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetFullName(SetFullNameRequest request)
        {
            var result = await _memberDashboardBll.SetFullName(request.CustId, request.FullName);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SetEmail 修改用戶Email
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetEmail(SetEmailRequest request)
        {
            var result = await _memberDashboardBll.SetEmail(request.CustId, request.Email);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SetPhoneNo 修改用戶電話
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetPhoneNo(SetPhoneNoRequest request)
        {
            var result = await _memberDashboardBll.SetPhoneNo(request.CustId, request.PhoneNo);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SetBirthday 修改用戶生日
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetBirthday(SetBirthdayRequest request)
        {
            var result = await _memberDashboardBll.SetBirthday(request.CustId, request.BirthYear, request.BirthMonth, request.BirthDate);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SetAddress 修改會員地址
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetAddress(SetMemberAddressRequest request)
        {
            var result = await _memberDashboardBll.SetAddress(request.CustId, request.Country, request.State, request.PostCode, request.City, request.Line1, request.Line2);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SetKycStatus 修改會員Kyc狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetKycStatus(SetMemberKycStatusRequest request)
        {
            var result = await _memberDashboardBll.SetKycStatus(request.CustId, request.Status);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SetRiskStatus 修改會員Risk狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SetRiskStatus(SetMemberRiskStatusRequest request)
        {
            var result = await _memberDashboardBll.SetRiskStatus(request.CustId, request.Status);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SwitchEmailVerified 切換會員Email驗證狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SwitchEmailVerified(SwitchMemberEmailVerifiedRequest request)
        {
            var result = await _memberDashboardBll.SwitchEmailVerified(request.CustId, request.Status);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/SwitchPhoneVerified 切換會員電話驗證狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResult> SwitchPhoneVerified(SwitchMemberPhoneVerifiedRequest request)
        {
            var result = await _memberDashboardBll.SwitchPhoneVerified(request.CustId, request.Status);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/GetDeviceList 取得會員使用過的裝置
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Member Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<MemberLoginDevice>>> GetDeviceList(GetIpListRequest request)
        {
            var data = await _memberDashboardBll.GetDeviceListByCustId(request.CustId);
            var apiResult = new ApiResult<IEnumerable<MemberLoginDevice>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
        
        /// <summary>
        /// Member/GetIpList 取得會員登入過的Ip
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Member Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<CustomerLoginIp>>> GetIpList(GetIpListRequest request)
        {
            var data = await _memberDashboardBll.GetIpListByCustId(request.CustId);
            var apiResult = new ApiResult<IEnumerable<CustomerLoginIp>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
    }
}