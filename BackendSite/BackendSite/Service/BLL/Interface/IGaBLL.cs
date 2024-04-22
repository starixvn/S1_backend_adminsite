using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IGaBLL
    {
        Task<ApiResult> PurchaseEventSendAsync(PurchaseRequest request);
    }
}
