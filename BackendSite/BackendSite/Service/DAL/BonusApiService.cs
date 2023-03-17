using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL;

public class BonusApiService : IBonusApiService
{
    private readonly CourierAdapter _courierAdapter;
    private readonly string _bonusApiUrl;

    public BonusApiService(CourierAdapter courierAdapter, IConfiguration configuration)
    {
        _courierAdapter = courierAdapter;
        _bonusApiUrl = configuration.GetValue<string>("BonusService");
    }
    
    public async Task<ApiResult> SetVerificationRewardAsync(int custId, int otpType)
    {
        var rewardUrl = $"{_bonusApiUrl}SetVerificationReward";
        var verificationReq = new
        {
            CustId = custId,
            OTPType = otpType
        };

        var apiResult = await _courierAdapter.PostAsync<ApiResult, object>(rewardUrl, verificationReq);
        return apiResult;
    }
}