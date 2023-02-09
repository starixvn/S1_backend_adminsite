using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface IBonusApiService
{
    Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType);
}