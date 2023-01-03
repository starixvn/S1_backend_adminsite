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
            startTime, endTime, absPageSize, startIndex,
            out var totalPages, out var rowCount);
        pageInfo = new PageInfo();
        pageInfo.ListPagination(rowCount, pageNumber, pageSize);

        return result;
    }

    public bool ModifyPaymentChargeFee(ModifyPaymentChargeFeeParams input)
    {
        var result = _chargeFeeService.CreateModifyPaymentChargeFeeLog(input);
        var success = result == (int) ErrorCodeEnum.Success;
        if (!success)
        {
            _logger.LogError($"Admin_ModifyPaymentChargeFeeLog_Ins failed, ResultCode({result}).");
        }

        return success;
    }
}