using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL;

public class PaymentChargeFeeBLL
{
    private readonly PaymentChargeFeeService _chargeFeeService;
    private readonly ILogger<PaymentChargeFeeBLL> _logger;

    public PaymentChargeFeeBLL(IServiceProvider provider)
    {
        _chargeFeeService = provider.GetRequiredService<PaymentChargeFeeService>();
        _logger = provider.GetRequiredService<ILogger<PaymentChargeFeeBLL>>();
    }

    public IEnumerable<ModifyPaymentChargeFeeLog> GetModifyPaymentChargeFeeLogList(int siteId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int pageNumber,
        int pageSize,
        out IPageInfo pageInfo)
    {
        var (startIndex, absPageSize) = pageNumber.GetStartIndex(pageSize);
        var result = _chargeFeeService.GetModifyPaymentChargeFeeLogList(siteId,
            startTime, endTime, startIndex, absPageSize,
            out var totalPages, out var rowCount);
        pageInfo = new PageInfo();
        pageInfo.ListPagination(rowCount, pageNumber, pageSize);

        return result;
    }

    public (ErrorCodeEnum Result, string Message) ModifyPaymentChargeFee(ModifyPaymentChargeFeeParams input)
    {
        var result = _chargeFeeService.CreateModifyPaymentChargeFeeLog(input);
        var success = result == (int)ErrorCodeEnum.Success;
        var msg = string.Empty;

        if (success)
            return (ErrorCodeEnum.Success, msg);

        msg = result switch
        {
            -1 => $"({result}) Channel id: {input.ChannelId} not exists.",
            -2 => $"({result})Method not support.",
            -3 => $"({result})No data needs to be updated.",
            -4 => $"({result})Failed to insert log.",
            _ => $"({result}) Unknown error."
        };

        _logger.LogError($"Admin_ModifyPaymentChargeFeeLog_Ins failed, Message: {msg}");

        return (ErrorCodeEnum.Fail, msg);
    }
}