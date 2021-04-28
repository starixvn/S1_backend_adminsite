using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoListForDisplay(bool isSetting, int siteId, int id)
        {
            return ConvertForDisplay(_bankFinancialService.GetTransferAccountInfoList(isSetting, siteId, id));
        }

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoListForExport(bool isSetting, int siteId, int id)
        {
            return ConvertForExport(_bankFinancialService.GetTransferAccountInfoList(isSetting, siteId, id));
        }

        public (bool Status, int Count, string Message) SetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            bool status = false;
            int count = 0;
            string message = string.Empty;

            if (!CheckParameterForSetTransferAccount(bankFinancialTransferAccount, out message))
            {
                return (status, count, message);
            }

            BankFinancialTransferAccountModel transferAccountModel = new BankFinancialTransferAccountModel
            {
                Id = bankFinancialTransferAccount.Id,
                SiteId = bankFinancialTransferAccount.SiteId,
                SiteName = bankFinancialTransferAccount.SiteName,
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

                    if (_bankFinancialService.InsertTransferAccountLog(ActionType.Update, transferAccountModel) != 1)
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

        #region Private Methods Set TransferAccount

        private bool CheckParameterForSetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount,
                                                         out string message)
        {
            message = String.Empty;

            if (bankFinancialTransferAccount == null)
            {
                message = "Data Invalid.";

                return false;
            }

            #region Parameter Values Check

            if (bankFinancialTransferAccount.Id == null)
            {
                message = "Data-[Id] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferAccount.SiteId == null)
            {
                message = "Data-[Site] invalid.";

                return false;

            }

            if (bankFinancialTransferAccount.SiteId != 0 &&
                bankFinancialTransferAccount.SiteId != 1 &&
                bankFinancialTransferAccount.SiteId != 2 &&
                bankFinancialTransferAccount.SiteId != 3)
            {
                message = "Data-[Site] not match.";

                return false;
            }

            if (bankFinancialTransferAccount.SiteName == null)
            {
                message = "Data-[SiteName] Invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.CurrencyId == null || bankFinancialTransferAccount.CurrencyId == 0)
            {
                message = "Data-[Currency] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.SysId == null || bankFinancialTransferAccount.SysId == 0)
            {
                message = "Data-[Bank Code] invalid.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(bankFinancialTransferAccount.Purpose))
            {
                message = "Data-[Purpose] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.Channel == null || bankFinancialTransferAccount.Channel == 0)
            {
                message = "Data-[Channel] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.Channel == TransferChannelEnum.PSP &&
                (bankFinancialTransferAccount.PayId == null || bankFinancialTransferAccount.PayId == 0))
            {
                message = "Data-[Channel] is PSP.But [Pay] is invalid.";

                return false;
            }

            if ((bankFinancialTransferAccount.Channel == TransferChannelEnum.LocalBank || bankFinancialTransferAccount.Channel == TransferChannelEnum.HeadOffice) &&
               bankFinancialTransferAccount.Agent == null)
            {
                message = "Data-[Channel] is Local Bank or Head Office.But [Agent] is invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.AccountNo == null)
            {
                message = "Data-[AccountNo] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.AccountName == null)
            {
                message = "Data-[AccountName] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.DisplayName == null)
            {
                message = "Data-[DisplayName] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.InitialAmount == null || bankFinancialTransferAccount.InitialAmount < 0)
            {
                message = "Data-[InitialAmount] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.MaxBalanceAlertAmount == null || bankFinancialTransferAccount.MaxBalanceAlertAmount < 0)
            {
                message = "Data-[MaxBalanceAlertAmount] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.Status == null)
            {
                message = "Data-[Status] invalid.";

                return false;
            }

            if (bankFinancialTransferAccount.CreateTime == null)
            {
                message = "Data-CreateTime invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferAccount.Creator == null)
            {
                message = "Data-Creator invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferAccount.UpdateTime == null)
            {
                message = "Data-UpdateTime invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferAccount.Updator == null)
            {
                message = "Data-Updator invalid.Notify admin please.";

                return false;
            }

            #endregion


            return true;
        }

        private string SortPurpose(string purpose)
        {

            IEnumerable<int> sortPurpose = purpose.Split(',').Select(int.Parse).ToList();
            sortPurpose = sortPurpose.OrderBy(p => p);
            var result = string.Join(",", sortPurpose);

            return result;
        }

        #endregion

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoList(bool isSetting,
                                                                                     TransferTypeEnum? transferType,
                                                                                     string transFromDate,
                                                                                     string transToDate,
                                                                                     string fromAccountNo,
                                                                                     string fromAccountDisplayName,
                                                                                     string toAccountNo,
                                                                                     string toAccountDisplayName,
                                                                                     int? reason,
                                                                                     string transferId)
        {
            return _bankFinancialService.GetTransferInfoList(isSetting,
                                                             transferType,
                                                             transFromDate,
                                                             transToDate,
                                                             fromAccountNo,
                                                             fromAccountDisplayName,
                                                             toAccountNo,
                                                             toAccountDisplayName,
                                                             reason,
                                                             transferId);
        }

        public (bool Status, int Count, string Message) SetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail)
        {
            bool status = false;
            int count = 0;
            string message = string.Empty;

            BankFinancialTransferInfoModel transferInfoFromModel = new BankFinancialTransferInfoModel
            {
                TransferId = bankFinancialTransferInfoDetail.TransferId,
                TransactionType = bankFinancialTransferInfoDetail.TransactionType,
                TransferType = bankFinancialTransferInfoDetail.TransferType,
                PlusOrMinus = PlusOrMinusEnum.Minus,
                TransDate = bankFinancialTransferInfoDetail.TransDate,
                Channel = bankFinancialTransferInfoDetail.FromChannel,
                Agent = bankFinancialTransferInfoDetail.FromAgent == "" ? null : bankFinancialTransferInfoDetail.FromAgent,
                PayId = bankFinancialTransferInfoDetail.FromPayId == 0 ? null : bankFinancialTransferInfoDetail.FromPayId,
                SiteId = bankFinancialTransferInfoDetail.FromSiteId,
                SiteName = bankFinancialTransferInfoDetail.FromSiteName == "" ? null : bankFinancialTransferInfoDetail.FromSiteName,
                CustId = bankFinancialTransferInfoDetail.FromCustId == 0 ? null : bankFinancialTransferInfoDetail.FromCustId,
                CurrencyId = bankFinancialTransferInfoDetail.FromCurrencyId,
                SysId = bankFinancialTransferInfoDetail.FromSysId,
                AccountNo = bankFinancialTransferInfoDetail.FromAccountNo,
                AccountDisplayName = bankFinancialTransferInfoDetail.FromAccountDisplayName,
                Amount = bankFinancialTransferInfoDetail.FromAmount,
                Fee = bankFinancialTransferInfoDetail.FromFee,
                Reason = bankFinancialTransferInfoDetail.Reason,
                Remark = bankFinancialTransferInfoDetail.Remark,
                CreateTime = bankFinancialTransferInfoDetail.CreateTime,
                Creator = bankFinancialTransferInfoDetail.Creator,
                UpdateTime = bankFinancialTransferInfoDetail.UpdateTime,
                Updator = bankFinancialTransferInfoDetail.Updator
            };

            BankFinancialTransferInfoModel transferInfoToModel = new BankFinancialTransferInfoModel
            {
                TransferId = bankFinancialTransferInfoDetail.TransferId,
                TransactionType = bankFinancialTransferInfoDetail.TransactionType,
                TransferType = bankFinancialTransferInfoDetail.TransferType,
                PlusOrMinus = PlusOrMinusEnum.Plus,
                TransDate = bankFinancialTransferInfoDetail.TransDate,
                Channel = bankFinancialTransferInfoDetail.ToChannel,
                Agent = bankFinancialTransferInfoDetail.ToAgent == "" ? null : bankFinancialTransferInfoDetail.ToAgent,
                PayId = bankFinancialTransferInfoDetail.ToPayId == 0 ? null : bankFinancialTransferInfoDetail.ToPayId,
                SiteId = bankFinancialTransferInfoDetail.ToSiteId,
                SiteName = bankFinancialTransferInfoDetail.ToSiteName == "" ? null : bankFinancialTransferInfoDetail.ToSiteName,
                CustId = bankFinancialTransferInfoDetail.ToCustId == 0 ? null : bankFinancialTransferInfoDetail.ToCustId,
                CurrencyId = bankFinancialTransferInfoDetail.ToCurrencyId,
                SysId = bankFinancialTransferInfoDetail.ToSysId,
                AccountNo = bankFinancialTransferInfoDetail.ToAccountNo,
                AccountDisplayName = bankFinancialTransferInfoDetail.ToAccountDisplayName,
                Amount = bankFinancialTransferInfoDetail.ToAmount,
                Fee = bankFinancialTransferInfoDetail.ToFee,
                Reason = bankFinancialTransferInfoDetail.Reason,
                Remark = bankFinancialTransferInfoDetail.Remark,
                CreateTime = bankFinancialTransferInfoDetail.CreateTime,
                Creator = bankFinancialTransferInfoDetail.Creator,
                UpdateTime = bankFinancialTransferInfoDetail.UpdateTime,
                Updator = bankFinancialTransferInfoDetail.Updator
            };

            if (transferInfoFromModel.TransferId == null)
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    string TransferId = GetTransferId(bankFinancialTransferInfoDetail.TransferType);
                    transferInfoFromModel.TransferId = TransferId;
                    transferInfoToModel.TransferId = TransferId;

                    if (_bankFinancialService.InsertTransferInfo(transferInfoFromModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Minus Data Fail.");
                    };

                    if (_bankFinancialService.InsertTransferInfo(transferInfoToModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Plus Data Fail.");
                    };

                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Create, transferInfoFromModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                    }

                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Create, transferInfoToModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                    }

                    scope.Complete();
                }
                status = true;
                count = 1;
                message = "Insert Transfer Info And Insert Log Success.";
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    if (_bankFinancialService.UpdateTransferInfo(transferInfoFromModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Update Transfer Info Minus Data Fail.");
                    };

                    if (_bankFinancialService.UpdateTransferInfo(transferInfoToModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Update Transfer Info Plus Data Fail.");
                    };

                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Update, transferInfoFromModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                    }

                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Update, transferInfoToModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                    }

                    scope.Complete();
                }
                status = true;
                count = 1;
                message = "Update Transfer Info And Insert Log Success.";
            }

            return (status, count, message);
        }

        public (bool Status, int Count, string Message) DeleteTransferInfo(string transferId)
        {

            bool status = false;
            int count = 0;
            string message = string.Empty;

            if (transferId != null)
            {
                //Step1:Get Data
                IEnumerable<BankFinancialTransferInfoDetailModel> getdata = _bankFinancialService.GetTransferInfoList(true,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             transferId);

                BankFinancialTransferInfoDetailModel data = new BankFinancialTransferInfoDetailModel();

                if (getdata.Count()==1)
                {
                    data = getdata.FirstOrDefault();
                }

                BankFinancialTransferInfoModel dataFromModel = new BankFinancialTransferInfoModel
                {
                    TransferId = data.TransferId,
                    TransactionType = data.TransactionType,
                    TransferType = data.TransferType,
                    PlusOrMinus = PlusOrMinusEnum.Minus,
                    TransDate = data.TransDate,
                    Channel = data.FromChannel,
                    Agent = data.FromAgent == "" ? null : data.FromAgent,
                    PayId = data.FromPayId == 0 ? null : data.FromPayId,
                    SiteId = data.FromSiteId,
                    SiteName = data.FromSiteName == "" ? null : data.FromSiteName,
                    CustId = data.FromCustId == 0 ? null : data.FromCustId,
                    CurrencyId = data.FromCurrencyId,
                    SysId = data.FromSysId,
                    AccountNo = data.FromAccountNo,
                    AccountDisplayName = data.FromAccountDisplayName,
                    Amount = data.FromAmount,
                    Fee = data.FromFee,
                    Reason = data.Reason,
                    Remark = data.Remark,
                    CreateTime = data.CreateTime,
                    Creator = data.Creator,
                    UpdateTime = data.UpdateTime,
                    Updator = data.Updator
                };

                BankFinancialTransferInfoModel dataToModel = new BankFinancialTransferInfoModel
                {
                    TransferId = data.TransferId,
                    TransactionType = data.TransactionType,
                    TransferType = data.TransferType,
                    PlusOrMinus = PlusOrMinusEnum.Plus,
                    TransDate = data.TransDate,
                    Channel = data.ToChannel,
                    Agent = data.ToAgent == "" ? null : data.ToAgent,
                    PayId = data.ToPayId == 0 ? null : data.ToPayId,
                    SiteId = data.ToSiteId,
                    SiteName = data.ToSiteName == "" ? null : data.ToSiteName,
                    CustId = data.ToCustId == 0 ? null : data.ToCustId,
                    CurrencyId = data.ToCurrencyId,
                    SysId = data.ToSysId,
                    AccountNo = data.ToAccountNo,
                    AccountDisplayName = data.ToAccountDisplayName,
                    Amount = data.ToAmount,
                    Fee = data.ToFee,
                    Reason = data.Reason,
                    Remark = data.Remark,
                    CreateTime = data.CreateTime,
                    Creator = data.Creator,
                    UpdateTime = data.UpdateTime,
                    Updator = data.Updator
                };

                //Step2:Insert Log


                //Step3:Delete Data

                using (TransactionScope scope = new TransactionScope())
                {
                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Delete, dataFromModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                    }

                    if (_bankFinancialService.InsertTransferInfoLog(ActionType.Delete, dataToModel) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                    }

                    if (_bankFinancialService.DeleteTransferInfo(transferId) != 1)
                    {
                        scope.Dispose();

                        return (status, count, "Delete Transfer Info Data Fail.");
                    }

                    scope.Complete();

                    count = 1;
                }
                status = true;
                message = "Delete Transfer Info Success.";
            }
            else
            {
                return (status, count, "Delete Transfer Info Data Fail.TransferId is null.");
            }

            return (status, count, message);

        }

        #region Private Methods For GetTransferAccountInfoList

        private IEnumerable<BankFinancialTransferAccountDetailModel> ConvertForDisplay(IEnumerable<BankFinancialTransferAccountDetailModel> datas)
        {

            foreach (var data in datas.ToArray())
            {

                data.CreateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.CreateTime);
                data.UpdateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.UpdateTime);
            }

            return datas;

        }

        private IEnumerable<BankFinancialTransferAccountDetailModel> ConvertForExport(IEnumerable<BankFinancialTransferAccountDetailModel> datas)
        {

            foreach (var data in datas.ToArray())
            {

                data.Purpose = GetPurpose(data.Purpose);

                data.CreateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.CreateTime);
                data.UpdateTime = GetTimeZoneDateTimeByDateString(data.SiteId, data.UpdateTime);
            }

            return datas;

        }

        private string GetTimeZoneDateTimeByDateString(int siteId, string datetime)
        {
            int timeZone = (siteId == 2 || siteId == 3) ? 7 : 8;
            return Convert.ToDateTime(datetime).AddHours(timeZone).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string GetPurpose(string purpose)
        {
            string result = string.Empty;

            IEnumerable<int> purposeList = purpose.Split(',').Select(int.Parse).ToList();

            foreach (int i in purposeList)
            {
                switch (i)
                {
                    case 1: result = result + ",Deposit"; break;
                    case 2: result = result + ",Withdrawal"; break;
                    case 3: result = result + ",Transfer"; break;
                }

            }

            return result.Substring(1);
        }

        private string GetTransferId(TransferTypeEnum transferType)
        {
            string prefix = string.Empty;
            string nowDate = string.Empty;
            string nowMaxNo = string.Empty;

            switch (transferType)
            {
                case TransferTypeEnum.Deposit:
                    prefix = "D";
                    break;
                case TransferTypeEnum.Withdrawal:
                    prefix = "W";
                    break;
                case TransferTypeEnum.Transfer:
                    prefix = "T";
                    break;
                case TransferTypeEnum.Other:
                    prefix = "R";
                    break;
            }

            (nowDate, nowMaxNo) = _bankFinancialService.NoAssign(prefix);

            return prefix + nowDate + nowMaxNo.PadLeft(4, '0');

        }

        #endregion

    }
}
