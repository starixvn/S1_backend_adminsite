using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingWithdrawalAmountBLL
    {
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;

        public MpaMarketingWithdrawalAmountBLL(IMpaMarketingWithdrawalAmountService withdrawalAmountService)
        {
            this._withdrawalAmountService = withdrawalAmountService;
        }

        public async Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request)
        {
            var withSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.StartDate,
                request.EndDate)).ToList();

            var compareWithSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.CompareFromDate,
                request.CompareToDate)).ToList();

            var response = new WithdrawalAmountSummaryResponse();
            response.WithdrawalAmount = withSummaryResult.Sum(x => x.WithdrawalAmount);
            response.CompareWithdrawalAmount = compareWithSummaryResult.Sum(x => x.WithdrawalAmount);
            response.WithdrawalUsers = withSummaryResult.DistinctBy(x => x.CustId).Count();
            response.CompareWithdrawalUsers = compareWithSummaryResult.DistinctBy(x => x.CustId).Count();
            //response.WithdrawalAmountChangeRate = 

            return null;
        }



        
    }
}
