using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.BankTransfer.Process;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class BankTransferController : ControllerBase
{
    #region Constructors

    public BankTransferController(IServiceProvider provider)
    {
        _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
        _excelBll = provider.GetRequiredService<ExcelBLL>();
        _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
    }

    #endregion

    #region Fields

    private readonly NewAuthorizeBLL _authorizeBll;
    private readonly BankFinancialBLL _bankFinancialBll;
    private readonly ExcelBLL _excelBll;
    private readonly ServerInfoBLL _serverInfoBll;

    #endregion

    #region API Model

    public class TransferIdRequest
    {
        public string TransferId { get; set; }
    }

    #endregion

    #region Action

    /// <summary>
    ///     BankTransfer/GetList 取得銀行轉帳資料
    /// </summary>
    /// <remarks>
    ///     Http Method：POST
    ///     Func Path：Bank Transfer > Transfer Info Management > [Submit](Query)
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public ApiListResult<BankFinancialTransferInfoTransDetailModel> GetList(GetTransferInfoListRequest request)
    {
        var result = _bankFinancialBll.GetFinancialTransferInfoList(
            new GetTransferInfoListParams(request), request.PageNumber, request.PageSize, out var pageInfo);

        foreach (var data in result)
        {
            data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
        }

        return new ApiListResult<BankFinancialTransferInfoTransDetailModel>(ErrorCodeEnum.Success, pageInfo, result);
    }

    /// <summary>
    ///     BankTransfer/Export 匯出銀行轉帳資料
    /// </summary>
    /// <remarks>
    ///     Http Method：POST
    ///     Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Export Excel]
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Export(ExportTransferInfoListRequest request)
    {
        var offset = request.StartTime.Offset;
        var exportData = _bankFinancialBll.ExportFinancialTransferInfoList(
            new GetTransferInfoListParams(request));

        var stream = _excelBll.Export(exportData, offset: offset);
        return _excelBll.ExcelFile(stream, "TransferInfoList");
    }

    /// <summary>
    ///     BankTransfer/GetByTransferId 取得指定銀行轉帳資料
    /// </summary>
    /// <remarks>
    ///     Http Method：POST
    ///     Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Edit]
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public ApiResult<BankFinancialTransferInfoTransDetailModel> GetByTransferId(TransferIdRequest request)
    {
        var apiResult = new ApiResult<BankFinancialTransferInfoTransDetailModel>();
        var result = _bankFinancialBll.GetFinancialTransferInfoById(request.TransferId);

        if (result != null)
        {
            var receipt = _bankFinancialBll.GetTransferIngoReceipt(result.TransferId);
            var authData = _authorizeBll.GetAuthData();
            result.ReceiptUrl = receipt?.PicFileName;
            result.Updator = authData.UserId;
            result.UpdatorName = authData.UserName;

            apiResult.Data = result;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        }
        else
        {
            throw new AdminException(ErrorCodeEnum.Fail, "The Transfer Account Detail Is Not Exist.");
        }

        return apiResult;
    }

    /// <summary>
    ///     BankTransfer/Set 新增/更新指定銀行轉帳資料
    /// </summary>
    /// <remarks>
    ///     Http Method：POST
    ///     Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Edit] > [Submit]
    ///     Bank Transfer > Transfer Info Management > [Create] > [Submit]
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> Set([FromForm] BankTransferInfoSetRequest request)
    {
        var authData = _authorizeBll.GetAuthData();
        var siteName = _serverInfoBll.GetServerInfo(request.SiteId).SiteName;
        var detail = new BankFinancialTransferInfoTransDetailModel
        {
            TransferId = request.TransferId,
            TransDate = request.TransDate,
            TransactionType = request.TransactionType,
            TransferType = request.TransferType,
            FromSiteId = request.SiteId,
            FromSiteName = siteName,
            ToSiteId = request.SiteId,
            ToSiteName = siteName,
            FromChannel = request.FromChannel,
            FromAgent = request.FromAgent,
            FromPayId = request.FromPayId,
            FromCurrencyId = request.FromCurrencyId,
            FromSysId = request.FromSysId,
            FromAccountNo = request.FromAccountNo,
            FromAccountDisplayName = request.FromAccountDisplayName,
            FromAmount = request.FromAmount,
            FromFee = request.FromFee,
            ToChannel = request.ToChannel,
            ToAgent = request.ToAgent,
            ToPayId = request.ToPayId,
            ToCurrencyId = request.ToCurrencyId,
            ToSysId = request.ToSysId,
            ToAccountNo = request.ToAccountNo,
            ToAccountDisplayName = request.ToAccountDisplayName,
            ToAmount = request.ToAmount,
            ToFee = request.ToFee,
            Reason = request.Reason,
            Remark = request.Remark,
            Updator = authData.UserId,
            UpdatorName = authData.UserName
        };

        var (success, count, message) = await _bankFinancialBll.SetTransferInfo(detail, request.UploadReceipt);

        return new ApiResult
        {
            ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail),
            Message = message
        };
    }

    /// <summary>
    ///     BankTransfer/Delete 刪除指定銀行轉帳資料
    /// </summary>
    /// <remarks>
    ///     Http Method：POST
    ///     Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Delete]
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="AdminException"></exception>
    [HttpPost]
    public ApiResult Delete(TransferIdRequest request)
    {
        var apiResult = new ApiResult();
        var authData = _authorizeBll.GetAuthData();
        var (status, count, message) = _bankFinancialBll.DeleteTransferInfo(request.TransferId, authData.UserName);

        if (status)
        {
            apiResult.RawMessage = message;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        }
        else
        {
            throw new AdminException(ErrorCodeEnum.Fail, message);
        }

        return new ApiResult { ErrorCode = (int)ErrorCodeEnum.Success, Message = message };
    }

    [HttpPost]
    public ApiResult<IEnumerable<SelectListItem>> GetTransferTypeList()
    {
        var except = new[]
        {
            TransferTypeEnum.Undefined,
            TransferTypeEnum.Deposit,
            TransferTypeEnum.Withdrawal
        };

        return new ApiResult<IEnumerable<SelectListItem>>
        {
            ErrorCode = (int)ErrorCodeEnum.Success,
            Data = EnumExtension.EnumToSelectListItem(except)
        };
    }

    [HttpPost]
    public ApiResult<IEnumerable<SelectListItem>> GetTransactionTypeList()
    {
        return new ApiResult<IEnumerable<SelectListItem>>
        {
            ErrorCode = (int)ErrorCodeEnum.Success,
            Data = EnumExtension.EnumToSelectListItem<TransactionTypeEnum>()
        };
    }

    #endregion
}
