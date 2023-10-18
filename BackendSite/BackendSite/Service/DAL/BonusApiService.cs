using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.DAL;

public class BonusApiService : IBonusApiService
{
    private readonly CourierAdapter _courierAdapter;
    private readonly string _bonusApiUrl;

    public BonusApiService(CourierAdapter courierAdapter, ConfigBLL configBll)
    {
        _courierAdapter = courierAdapter;
        _bonusApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").BonusService;
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

    public async Task<ApiResult> CheckRewardCondination(int custId, int otpType)
    {
        var apiResult = new ApiResult();
        var myUrl = $"{_bonusApiUrl}CheckRewardCondination";
        var verificationReq = new SetVerificationReq()
        {
            CustId = custId,
            OTPType = otpType
        };
        var data = await _courierAdapter.MyPostAsync<ApiResult, SetVerificationReq>(myUrl, verificationReq);

        return data;
    }
}