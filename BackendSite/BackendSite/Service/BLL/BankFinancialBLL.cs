using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountList()
        {
            return _bankFinancialService.GetTransferAccountList();
        }


        #endregion

        #region Transfer Account Management

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

            if (bankFinancialTransferAccount.SiteId != 4 &&
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

        #endregion

        #region Transfer Info Management

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
            return ConvertForDisplay(_bankFinancialService.GetTransferInfoList(isSetting,
                                                             transferType,
                                                             transFromDate,
                                                             transToDate,
                                                             fromAccountNo,
                                                             fromAccountDisplayName,
                                                             toAccountNo,
                                                             toAccountDisplayName,
                                                             reason,
                                                             transferId));
        }

        public (bool Status, int Count, string Message) SetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail, IFormFile transferReceipt)
        {
            bool status = false;
            int count = 0;
            string message = string.Empty;

            #region Checnk Parameter

            if (!CheckSharedParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
            {
                return (status, count, message);
            }

            if (bankFinancialTransferInfoDetail.TransferType != TransferTypeEnum.Other)
            {
                if (!CheckParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (status, count, message);
                }
            }
            else
            {
                if (!CheckParameterWithTypeOtherForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (status, count, message);
                }
            }

            #endregion

            #region SetParameterModel

            var result = SetParameterModelForSetTransferInfo(bankFinancialTransferInfoDetail);

            #endregion

            if (bankFinancialTransferInfoDetail.TransferId == null)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    string TransferId = GetTransferId(bankFinancialTransferInfoDetail.TransferType);
                    result.TransferInfoFromModel.TransferId = TransferId;
                    result.TransferInfoToModel.TransferId = TransferId;

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(result.TransferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Minus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Create, result.TransferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(result.TransferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Plus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Create, result.TransferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (InsertTransferReceipt(transferReceipt, TransferId) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Receipt Fail.");
                        }
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
                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {

                        if (_bankFinancialService.UpdateTransferInfo(result.TransferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Update Transfer Info Minus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Update, result.TransferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.UpdateTransferInfo(result.TransferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Update Transfer Info Plus Data Fail.");
                        };


                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Update, result.TransferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (InsertTransferReceipt(transferReceipt, bankFinancialTransferInfoDetail.TransferId) > 2)
                        {
                            scope.Dispose();

                            return (status, count, "Update Transfer Receipt Fail.");
                        }
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

                var transferInfoReceiptModel= _bankFinancialService.GetTransferInfoReceipt(transferId);

                BankFinancialTransferInfoDetailModel data = new BankFinancialTransferInfoDetailModel();

                if (getdata.Count() == 1)
                {
                    data = getdata.FirstOrDefault();
                }
                else
                {
                    return (status, count, "Delete Transfer Info Data Fail.Data is not exist.");
                }

                //Step2: Set Data for Insert Log
                var result = SetParameterModelForSetTransferInfo(data);

                using (TransactionScope scope = new TransactionScope())
                {
                    int affectCount = 0;

                    //Step3:Insert Log
                    if (data.TransactionType == TransactionTypeEnum.PlusAndMinus
                       || data.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {
                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Delete, result.TransferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                        affectCount++;
                    }
                    if (data.TransactionType == TransactionTypeEnum.PlusAndMinus
                       || data.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.InsertTransferInfoLog(ActionType.Delete, result.TransferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                        affectCount++;
                    }

                    //Step4:Delete Data
                    if (_bankFinancialService.DeleteTransferInfo(transferId) != affectCount)
                    {
                        scope.Dispose();

                        return (status, count, "Delete Transfer Info Data Fail.");
                    }

                    if(transferInfoReceiptModel!=null)
                    {
                        if (_bankFinancialService.DeleteTransferInfoReceipt(transferId) != 1)
                        {
                            scope.Dispose();

                            return (status, count, "Delete Transfer Info Receipt Fail.");
                        }
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

        #region Private Methods For Set Transfer Info

        /// <summary>
        /// 檢核 共有 資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckSharedParameterForSetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail,
                                                            out string message)
        {
            message = String.Empty;

            if (bankFinancialTransferInfoDetail == null)
            {
                message = "Data Invalid.";

                return false;
            }

            #region Parameter Values Check

            if (bankFinancialTransferInfoDetail.TransferType == null
                || bankFinancialTransferInfoDetail.TransferType == 0)
            {
                message = "Data-[Transfer Type] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.TransactionType == null
                || bankFinancialTransferInfoDetail.TransactionType == 0)
            {
                message = "Data-[Transaction Type] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.TransDate == null)
            {
                message = "Data-[Trans Date] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.CreateTime == null)
            {
                message = "Data-[CreateTime] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.Creator == null)
            {
                message = "Data-[Creator] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.UpdateTime == null)
            {
                message = "Data-[UpdateTime] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.Updator == null)
            {
                message = "Data-[Updator] invalid.Notify admin please.";

                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 檢核 加項 和 減項 的資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckParameterForSetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail,
                                                      out string message)
        {
            message = String.Empty;

            #region From Data

            if (bankFinancialTransferInfoDetail.FromChannel == null || bankFinancialTransferInfoDetail.FromChannel == 0)
            {
                message = "Data-[From Channel] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.PSP &&
                (bankFinancialTransferInfoDetail.FromPayId == null || bankFinancialTransferInfoDetail.FromPayId == 0))
            {
                message = "Data-[From Channel] is PSP.But [From Pay] is invalid.";

                return false;
            }

            if ((bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.HeadOffice) &&
               bankFinancialTransferInfoDetail.FromAgent == null)
            {
                message = "Data-[From Channel] is Local Bank or Head Office.But [From Agent] is invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromSiteId == null)
            {
                message = "Data-[From Site] invalid.";

                return false;

            }

            if (bankFinancialTransferInfoDetail.FromSiteId != 4 &&
                bankFinancialTransferInfoDetail.FromSiteId != 1 &&
                bankFinancialTransferInfoDetail.FromSiteId != 2 &&
                bankFinancialTransferInfoDetail.FromSiteId != 3)
            {
                message = "Data-[From Site] not match.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromSiteName == null)
            {
                message = "Data-[From SiteName] Invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromCurrencyId == null || bankFinancialTransferInfoDetail.FromCurrencyId == 0)
            {
                message = "Data-[From Currency] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromSysId == null || bankFinancialTransferInfoDetail.FromSysId == 0)
            {
                message = "Data-[From Bank Code] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromAccountNo == null)
            {
                message = "Data-[From Account No.] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromAccountDisplayName == null)
            {
                message = "Data-[From Account Display Name] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromAmount == null || bankFinancialTransferInfoDetail.FromAmount < 0)
            {
                message = "Data-[From Amount] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromFee == null || bankFinancialTransferInfoDetail.FromFee < 0)
            {
                message = "Data-[From Fee] invalid.";

                return false;
            }

            #endregion

            #region To Data

            if (bankFinancialTransferInfoDetail.ToChannel == null || bankFinancialTransferInfoDetail.ToChannel == 0)
            {
                message = "Data-[To Channel] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.PSP &&
                (bankFinancialTransferInfoDetail.ToPayId == null || bankFinancialTransferInfoDetail.ToPayId == 0))
            {
                message = "Data-[To Channel] is PSP.But [To Pay] is invalid.";

                return false;
            }

            if ((bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.HeadOffice) &&
               bankFinancialTransferInfoDetail.ToAgent == null)
            {
                message = "Data-[To Channel] is Local Bank or Head Office.But [To Agent] is invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteId == null)
            {
                message = "Data-[To Site] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteId != 4 &&
                bankFinancialTransferInfoDetail.ToSiteId != 1 &&
                bankFinancialTransferInfoDetail.ToSiteId != 2 &&
                bankFinancialTransferInfoDetail.ToSiteId != 3)
            {
                message = "Data-[To Site] not match.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteName == null)
            {
                message = "Data-[To SiteName] Invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToCurrencyId == null || bankFinancialTransferInfoDetail.ToCurrencyId == 0)
            {
                message = "Data-[To Currency] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSysId == null || bankFinancialTransferInfoDetail.ToSysId == 0)
            {
                message = "Data-[To Bank Code] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToAccountNo == null)
            {
                message = "Data-[To Account No.] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToAccountDisplayName == null)
            {
                message = "Data-[To Account Display Name] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToAmount == null || bankFinancialTransferInfoDetail.ToAmount < 0)
            {
                message = "Data-[To Amount] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToFee == null || bankFinancialTransferInfoDetail.ToFee < 0)
            {
                message = "Data-[To Fee] invalid.";

                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 檢核 和 判斷 單加項 或 單減項 的資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="transactionType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckParameterWithTypeOtherForSetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail,
                                                                   out string message)
        {
            message = String.Empty;

            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
            {
                //Minus

                if (bankFinancialTransferInfoDetail.FromChannel == null || bankFinancialTransferInfoDetail.FromChannel == 0)
                {
                    message = "Data-[From Channel] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.PSP &&
               (bankFinancialTransferInfoDetail.FromPayId == null || bankFinancialTransferInfoDetail.FromPayId == 0))
                {
                    message = "Data-[From Channel] is PSP.But [From Pay] is invalid.";

                    return false;
                }

                if ((bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.HeadOffice) &&
                   bankFinancialTransferInfoDetail.FromAgent == null)
                {
                    message = "Data-[From Channel] is Local Bank or Head Office.But [From Agent] is invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromSiteId == null)
                {
                    message = "Data-[From Site] invalid.";

                    return false;

                }

                if (bankFinancialTransferInfoDetail.FromSiteId != 4 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 1 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 2 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 3)
                {
                    message = "Data-[From Site] not match.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromSiteName == null)
                {
                    message = "Data-[From SiteName] Invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromCurrencyId == null || bankFinancialTransferInfoDetail.FromCurrencyId == 0)
                {
                    message = "Data-[From Currency] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromSysId == null || bankFinancialTransferInfoDetail.FromSysId == 0)
                {
                    message = "Data-[From Bank Code] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromAccountNo == null)
                {
                    message = "Data-[From Account No.] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromAccountDisplayName == null)
                {
                    message = "Data-[From Account Display Name] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromAmount == null || bankFinancialTransferInfoDetail.FromAmount < 0)
                {
                    message = "Data-[From Amount] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromFee == null || bankFinancialTransferInfoDetail.FromFee < 0)
                {
                    message = "Data-[From Fee] invalid.";

                    return false;
                }
            }
            else
            {
                //Plus

                if (bankFinancialTransferInfoDetail.ToChannel == null || bankFinancialTransferInfoDetail.ToChannel == 0)
                {
                    message = "Data-[To Channel] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.PSP &&
                    (bankFinancialTransferInfoDetail.ToPayId == null || bankFinancialTransferInfoDetail.ToPayId == 0))
                {
                    message = "Data-[To Channel] is PSP.But [To Pay] is invalid.";

                    return false;
                }

                if ((bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.HeadOffice) &&
                   bankFinancialTransferInfoDetail.ToAgent == null)
                {
                    message = "Data-[To Channel] is Local Bank or Head Office.But [To Agent] is invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteId == null)
                {
                    message = "Data-[To Site] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteId != 4 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 1 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 2 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 3)
                {
                    message = "Data-[To Site] not match.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteName == null)
                {
                    message = "Data-[To SiteName] Invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToCurrencyId == null || bankFinancialTransferInfoDetail.ToCurrencyId == 0)
                {
                    message = "Data-[To Currency] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSysId == null || bankFinancialTransferInfoDetail.ToSysId == 0)
                {
                    message = "Data-[To Bank Code] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToAccountNo == null)
                {
                    message = "Data-[To Account No.] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToAccountDisplayName == null)
                {
                    message = "Data-[To Account Display Name] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToAmount == null || bankFinancialTransferInfoDetail.ToAmount < 0)
                {
                    message = "Data-[To Amount] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToFee == null || bankFinancialTransferInfoDetail.ToFee < 0)
                {
                    message = "Data-[To Fee] invalid.";

                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// 裝載 加項設定資料 和 減項設定資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <returns></returns>
        private (BankFinancialTransferInfoModel TransferInfoFromModel, BankFinancialTransferInfoModel TransferInfoToModel) SetParameterModelForSetTransferInfo(BankFinancialTransferInfoDetailModel bankFinancialTransferInfoDetail)
        {
            var transferInfoFromModel = new BankFinancialTransferInfoModel();
            var transferInfoToModel = new BankFinancialTransferInfoModel();

            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus
                || bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
            {
                transferInfoFromModel = new BankFinancialTransferInfoModel
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
            }
            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus
               || bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
            {
                transferInfoToModel = new BankFinancialTransferInfoModel
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
            }

            return (transferInfoFromModel, transferInfoToModel);

        }

        #endregion

        #region Receipt Logic

        public BankFinancialTransferInfoReceiptModel GetTransferIngoReceipt(string transferId)
        {
            return _bankFinancialService.GetTransferInfoReceipt(transferId);
        }

        private int InsertTransferReceipt(IFormFile transferReceipt, string transferId)
        {
            var processedReceipt = ProcessImage(transferReceipt);

            return _bankFinancialService.SetTransferInfoReceipt(transferId, processedReceipt.FileName, processedReceipt.FileBytes);
        }

        public ProcessedImage ProcessImage(IFormFile image)
        {
            var processedImage = new ProcessedImage();
            
            if (_contentTypesCheck.ContainsKey(image.ContentType))
            {
                decimal xSize = 500m;
                var apiResult = new ApiResult();
                decimal xCell = 1;

                using var fileStream = image.OpenReadStream();
                using var uploadedImage = Image.FromStream(fileStream);
                using var ms = new System.IO.MemoryStream();

                if (uploadedImage.Width > xSize || uploadedImage.Height > xSize)
                {
                    if (uploadedImage.Width > uploadedImage.Height)
                    {
                        xCell = xSize / uploadedImage.Width;
                    }
                    else
                    {
                        xCell = xSize / uploadedImage.Height;
                    }
                }

                var graphicOptions = new GraphicOptions();
                graphicOptions.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                graphicOptions.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                string fileName = image.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                byte[] fileBytes = ms.ToArray();

                processedImage.FileName = fileName;
                processedImage.FileBytes = fileBytes;
            }

            return processedImage;
        }

        public class ProcessedImage
        {
            public string FileName { get; set; }
            public byte[] FileBytes { get; set; }
        }

        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck = new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

        #endregion

        /// <summary>
        /// 時區轉換
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private IEnumerable<BankFinancialTransferInfoDetailModel> ConvertForDisplay(IEnumerable<BankFinancialTransferInfoDetailModel> datas)
        {

            foreach (var data in datas.ToArray())
            {
                data.CreateTime = GetTimeZoneDateTimeByDateString(1, data.CreateTime);
                data.UpdateTime = GetTimeZoneDateTimeByDateString(1, data.UpdateTime);
            }

            return datas;

        }

        #endregion

    }
}
