using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace BackendSite.Service.BLL
{
    public class BankFinancialBLL
    {
        #region Fields

        private readonly BankFinancialService _bankFinancialService;
        private readonly PaymentManageService _paymentManageService;
        private readonly SelLangBLL _selLangBLL;

        #endregion

        #region Constructors

        public BankFinancialBLL(BankFinancialService bankFinancialService,
                                PaymentManageService paymentManageService,
                                SelLangBLL selLangBLL)
        {
            _bankFinancialService = bankFinancialService;
            _paymentManageService = paymentManageService;
            _selLangBLL = selLangBLL;
        }


        #endregion

        #region Library Formula

        /// <summary>
        /// 亞洲四捨五入
        /// </summary>
        /// <param name="number">判斷數字</param>
        /// <param name="point">小數點取位</param>
        /// <returns>四捨五入過後的數字</returns>
        private decimal RoundingNumber(decimal number, int point)
        {
            return Math.Round(number, point, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 數字轉換為百分比
        /// </summary>
        /// <param name="number">數字</param>
        /// <returns>百分比文字</returns>
        private string TransformToPercentage(decimal number, int point)
        {
            return Math.Round(number * 100, point, MidpointRounding.AwayFromZero).ToString() + "%";
        }

        #endregion

        #region Library Data

        public IEnumerable<BankFinancialReasonModel> GetReason(string funcKey)
        {
            return _bankFinancialService.GetReason(funcKey);
        }

        public IEnumerable<PayList> GetPayList()
        {
            return _paymentManageService.GetPayList();
        }

        #endregion

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoList(bool isSetting, int siteId, int id)
        {
            return _bankFinancialService.GetTransferAccountInfoList(isSetting, siteId, id);
        }

        public (bool Status,int Count,string Message) SetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            bool status = false;
            int count = 0;
            string message = string.Empty;



            BankFinancialTransferAccountModel transferAccountModel = new BankFinancialTransferAccountModel
            {
                Id = bankFinancialTransferAccount.Id,
                SiteId = bankFinancialTransferAccount.SiteId,
                CurrencyId = bankFinancialTransferAccount.CurrencyId,
                SysId = bankFinancialTransferAccount.SysId,
                Purpose = SortPurpose(bankFinancialTransferAccount.Purpose),
                Channel = bankFinancialTransferAccount.Channel,
                Agent = bankFinancialTransferAccount.Agent == "" ? null : bankFinancialTransferAccount.Agent,
                PayId = bankFinancialTransferAccount.PayId == 0 ? null : bankFinancialTransferAccount.PayId,
                AccountNo = bankFinancialTransferAccount.AccountNo,
                AccountName = bankFinancialTransferAccount.AccountName,
                DisplayName = bankFinancialTransferAccount.DisplayName,
                InitialAmount = bankFinancialTransferAccount.InitialAmount,
                MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                Reason = bankFinancialTransferAccount.Reason == 0 ? null : bankFinancialTransferAccount.Reason,
                Remark = bankFinancialTransferAccount.Remark,
                Status = bankFinancialTransferAccount.Status,
                CreateTime = bankFinancialTransferAccount.CreateTime,
                Creator = bankFinancialTransferAccount.Creator,
                UpdateTime = bankFinancialTransferAccount.UpdateTime,
                Updator = bankFinancialTransferAccount.Updator
            };

            if (transferAccountModel.Id == 0)
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    if (_bankFinancialService.InsertTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Account Fail.");
                    };

                    if (_bankFinancialService.InsertTransferAccountLog(ActionType.Create, transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Account Log Fail.");
                    }

                    scope.Complete();
                }
                status = true;
                count = 1;
                message = "Insert Transfer Account And Insert Log Success.";
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    if (_bankFinancialService.UpdateTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Update Transfer Account Fail.");
                    };

                    if (_bankFinancialService.InsertTransferAccountLog(ActionType.Create, transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Account Log Fail.");
                    }

                    scope.Complete();
                }
                status = true;
                count = 1;
                message = "Update Transfer Account And Insert Log Success.";
            }
            return (status, count, message);
        }


        private string SortPurpose(string purpose)
        {

            IEnumerable<int> sortPurpose = purpose.Split(',').Select(int.Parse).ToList();
            sortPurpose = sortPurpose.OrderBy(p => p);
            var result = string.Join(",", sortPurpose);

            return result;
        }

    }
}
