using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class WithdrawalAccountBLL : IWithdrawalAccountBLL
    {
        private readonly WithdrawalService _withdrawalService;

        public WithdrawalAccountBLL(WithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        /// <summary>
        /// 提款單狀態變更，Set 帳號提領成功次數
        /// </summary>
        /// <param name="withdrawalRequest"></param>
        /// <param name="changeToStatus"></param>
        /// <returns></returns>
        public async Task WithdrawalAccountCounter(WithdrawalRequest withdrawalRequest, WithdrawalStatusEnum changeToStatus)
        {
            var originStatus = withdrawalRequest.Status;
            var newStatus = changeToStatus;
            var accountNo = withdrawalRequest.AccountNo;
            var accountType = MapWithdrawalAccountType(withdrawalRequest.AccountType);
            var custId = withdrawalRequest.CustId;
            
            // 其他狀態變成 Approved 一律 +1
            if (newStatus == WithdrawalStatusEnum.Approved && originStatus != WithdrawalStatusEnum.Approved)
            {
                await _withdrawalService.SetWithdrawalAccountCounter(custId, accountNo, accountType, 1);
            }

            // 從 Approved 變更至其他狀態 一律 -1
            if (originStatus == WithdrawalStatusEnum.Approved && changeToStatus != WithdrawalStatusEnum.Approved)
            {
                await _withdrawalService.SetWithdrawalAccountCounter(custId, accountNo, accountType, -1);
            }

            //其他狀態之間的切換一律不用處理次數
        }


        public WithdrawalAccountTypeEnum MapWithdrawalAccountType(string accountType)
        {
            if (string.IsNullOrEmpty(accountType)) return WithdrawalAccountTypeEnum.BankCard;

            switch (accountType.ToLower())
            {
                case "ewallet":
                    return WithdrawalAccountTypeEnum.EWallet;
                case "crypto":
                    return WithdrawalAccountTypeEnum.Crypto;
            }

            throw new NotImplementedException("withdrawal accountType not found");
        }
    }
}
