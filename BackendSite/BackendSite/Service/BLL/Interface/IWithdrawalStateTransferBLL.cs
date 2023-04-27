using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IWithdrawalStateTransferBLL
{
    int WithdrawalToKyc(string transId, string comment, string updater);
    int WithdrawalOnHold(string transId, string comment, string updater);
    int WithdrawalDraft(string transId, string comment, IEnumerable<WithdrawalAccountingModel> accountingModels, string updater);
    int WithdrawalPending(string transId, string comment, string updater);
    int WithdrawalProcess(string transId, string comment, string updater, int userId, string withdrawalInfo,
        int? withdrawalType, IEnumerable<WithdrawalAccountingModel> accountingModels, out int custId, out decimal amount);
    int WithdrawalVerify(string transId, string comment, string updater);
    int WithdrawalApproved(string transId, string comment, string updater, int userId, bool hasPic);
    int WithdrawalRejected(string transId, string comment, string updater);
}