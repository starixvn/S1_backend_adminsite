using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Controllers.ApiV2.Member.Process;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class MemberController : ControllerBase
{
    private readonly ICustomerBLL _customerBll;
    private readonly ICustPhotoIdBLL _custPhotoIdBll;

    public MemberController(ICustomerBLL customerBll, ICustPhotoIdBLL custPhotoIdBll)
    {
        _customerBll = customerBll;
        _custPhotoIdBll = custPhotoIdBll;
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
        var data = await _customerBll.GetIpListByCustId(request.CustId);
        var apiResult = new ApiResult<IEnumerable<CustomerLoginIp>>()
        {
            ErrorCode = (int)ErrorCodeEnum.Success,
            Data = data
        };
        return apiResult;
    }
}