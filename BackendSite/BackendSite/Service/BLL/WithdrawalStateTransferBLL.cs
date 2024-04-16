using System.Text.RegularExpressions;

using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class WithdrawalStateTransferBLL : IWithdrawalStateTransferBLL
    {
        private readonly WithdrawalService _withdrawalService;

        public WithdrawalStateTransferBLL(WithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        public int WithdrawalToKyc(string transId, string comment, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Pending,
                WithdrawalStatusEnum.OnHold
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Pending And OnHold Can Set.");
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatusEnum.ToKyc, null, null, null, out _, out _);
        }

        public int WithdrawalOnHold(string transId, string comment, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Pending
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Pending Can Set.");
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatusEnum.OnHold, null, null, null, out _, out _);
        }

        public int WithdrawalDraft(string transId, string comment, IEnumerable<WithdrawalAccountingModel> accountingModels, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Pending,
                WithdrawalStatusEnum.OnHold,
                WithdrawalStatusEnum.Verify,
                WithdrawalStatusEnum.Process,
                WithdrawalStatusEnum.ToKyc,
                WithdrawalStatusEnum.Cancelled,
                WithdrawalStatusEnum.Rejected,
                WithdrawalStatusEnum.Approved
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatusEnum.Draft, null, null, accountingModels, out _, out _);
        }

        public int WithdrawalPending(string transId, string comment, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Verify,
                WithdrawalStatusEnum.ToKyc
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Verify, ToKyc Can Set.");
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatusEnum.Pending, null, null, null, out _, out _);
        }

        public int WithdrawalProcess(string transId, string comment, string updater, int userId, string withdrawalInfo,
            int? withdrawalType, IEnumerable<WithdrawalAccountingModel> accountingModels, out int custId, out decimal amount)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Verify
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Verify Can Set.");
            }

            var chargeFee = 0m;
            if (withdrawalType is (int)WithdrawalTypeEnum.CryptoWallet or (int)WithdrawalTypeEnum.LocalBank)
            {
                chargeFee = accountingModels.Sum(x => x.ChargeFee);
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatusEnum.Process, withdrawalInfo, withdrawalType, accountingModels, out custId, out amount, userId, chargeFee: chargeFee);
        }

        public int WithdrawalVerify(string transId, string comment, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.Pending,
                WithdrawalStatusEnum.OnHold,
                WithdrawalStatusEnum.Process

            };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Pending, OnHold, Process Can Set.");
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatusEnum.Verify, null, null, null, out _, out _);
        }

        public int WithdrawalApproved(string transId, string comment, string updater, int userId, bool hasPic)
        {
            var forStatus = new List<WithdrawalStatusEnum> { WithdrawalStatusEnum.Process };

            var withdrawalRequest = GetWithdrawalRequest(transId);
            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only Process Can Set.");
            }

            var withdrawalInfo = JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalRequest.WithdrawalInfo);
            withdrawalInfo.CurrencyId = withdrawalRequest.CurrencyId;
            if (withdrawalRequest.WithdrawalType == WithdrawalTypeEnum.CryptoWallet)
            {
                withdrawalInfo.ExchangeRate = withdrawalInfo.TempExchangeRate;
                withdrawalInfo.ExchangeAmount = withdrawalInfo.TempExchangeAmount;
            }
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatusEnum.Approved, JsonConvert.SerializeObject(withdrawalInfo),
                (int)withdrawalRequest.WithdrawalType, null, out _, out _, userId, hasPic);
        }

        public int WithdrawalRejected(string transId, string comment, string updater)
        {
            var forStatus = new List<WithdrawalStatusEnum>
            {
                WithdrawalStatusEnum.OnHold,
                WithdrawalStatusEnum.Verify,
                WithdrawalStatusEnum.ToKyc,
                WithdrawalStatusEnum.Pending,
                WithdrawalStatusEnum.Approved
            };

            var withdrawalRequest = GetWithdrawalRequest(transId);

            if (!CheckWithdrawalFlow(forStatus, withdrawalRequest.Status))
            {
                var status = (WithdrawalStatusEnum)withdrawalRequest.Status;
                throw new AdminException(ErrorCodeEnum.Fail,
                    $"This Withdrawal Flow is Wrong.This Status is {status.ToString()} and Only OnHold, Verify, ToKyc, Pending Can Set.");
            }

            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatusEnum.Rejected, null, null, null, out _, out _);
        }

        private WithdrawalRequest GetWithdrawalRequest(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            return _withdrawalService.GetWithdrawalRequest(withdrawalId);
        }

        private bool CheckWithdrawalFlow(IEnumerable<WithdrawalStatusEnum> approvedStatusList, WithdrawalStatusEnum status)
        {
            if (approvedStatusList.Contains(status))
            {
                return true;
            }
            return false;
        }

    }
}


