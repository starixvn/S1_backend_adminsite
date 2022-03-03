using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Transactions;

namespace BackendSite.Service.BLL
{
    public class BankFinancialBLL
    {
        #region Fields

        private readonly BankFinancialService _bankFinancialService;
        private readonly PaymentManageService _paymentManageService;
        private readonly ReportService _reportService;
        private readonly SelLangBLL _selLangBLL;

        #endregion

        #region Constructors

        public BankFinancialBLL(BankFinancialService bankFinancialService,
                                PaymentManageService paymentManageService,
                                ReportService reportService,
                                SelLangBLL selLangBLL)
        {
            _bankFinancialService = bankFinancialService;
            _paymentManageService = paymentManageService;
            _reportService = reportService;
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
            return _bankFinancialService.GetTransferAccountInfoList(isSetting, siteId, id);
        }

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoListForExport(bool isSetting, int siteId, int id)
        {
            return _bankFinancialService.GetTransferAccountInfoList(isSetting, siteId, id);
        }

        public (bool Status, int Count, string Message) SetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            var count = 0;
            var updateTransferDataFlag = false;
            var affectTransferData = 0;
            var oldData = new BankFinancialTransferAccountDetailModel();

            if (!CheckParameterForSetTransferAccount(bankFinancialTransferAccount, out var message))
            {
                return (false, count, message);
            }

            if (bankFinancialTransferAccount.Id != 0)
            {
                oldData = GetTransferAccountInfoListForDisplay(true, bankFinancialTransferAccount.SiteId, bankFinancialTransferAccount.Id).FirstOrDefault();

                affectTransferData = _bankFinancialService.GetTransferDataCount(oldData.AccountNo);

                if (affectTransferData > 0)
                {

                    updateTransferDataFlag = CheckKeyValueForSetTransferAccount(bankFinancialTransferAccount, oldData);
                }
            }

            var transferAccountModel = new BankFinancialTransferAccountModel
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
                using (var scope = new TransactionScope())
                {
                    if (_bankFinancialService.InsertTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Insert Transfer Account Fail.");
                    };

                    if (_bankFinancialService.InsertTransferAccountLog(ActionType.Create, transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Insert Transfer Account Log Fail.");
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Insert Transfer Account And Insert Log Success.";
            }
            else
            {
                using (var scope = new TransactionScope())
                {
                    if (_bankFinancialService.UpdateTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Update Transfer Account Fail.");
                    };

                    if (_bankFinancialService.InsertTransferAccountLog(ActionType.Update, transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Insert Transfer Account Log Fail.");
                    }

                    if (updateTransferDataFlag)
                    {
                        if (_bankFinancialService.UpdateTransferInfo(bankFinancialTransferAccount, oldData) != affectTransferData)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Data Fail.");
                        }
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Update Transfer Account And Insert Log Success.";
            }

            return (true, count, message);
        }

        #region Private Methods For GetTransferAccountInfoList

        private string GetTransferId(TransferTypeEnum transferType)
        {
            var prefix = string.Empty;
            var nowDate = string.Empty;
            var nowMaxNo = string.Empty;

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
            message = string.Empty;

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

        private bool CheckKeyValueForSetTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount, BankFinancialTransferAccountDetailModel oldData)
        {

            #region Key Values Check


            if (bankFinancialTransferAccount.CurrencyId != oldData.CurrencyId)
            {
                return true;
            }

            if (bankFinancialTransferAccount.SysId != oldData.SysId)
            {
                return true;
            }

            if (bankFinancialTransferAccount.Channel != oldData.Channel)
            {
                return true;
            }

            if (bankFinancialTransferAccount.PayId != oldData.PayId)
            {
                return true;
            }

            if (bankFinancialTransferAccount.Agent != oldData.Agent)
            {
                return true;
            }

            if (bankFinancialTransferAccount.AccountNo != oldData.AccountNo)
            {
                return true;
            }

            if (bankFinancialTransferAccount.AccountName != oldData.AccountName)
            {
                return true;
            }

            if (bankFinancialTransferAccount.DisplayName != oldData.DisplayName)
            {
                return true;
            }

            #endregion

            return false;
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

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> GetTransferInfoList(bool isSetting,
                                                                                     TransferTypeEnum? transferType,
                                                                                     DateTime? transFromDate,
                                                                                     DateTime? transToDate,
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

        public (bool Status, int Count, string Message) SetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail, IFormFile transferReceipt)
        {
            var count = 0;

            #region Checnk Parameter

            if (!CheckSharedParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out var message))
            {
                return (false, count, message);
            }

            if (bankFinancialTransferInfoDetail.TransferType != TransferTypeEnum.Other)
            {
                if (!CheckParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (false, count, message);
                }
            }
            else
            {
                if (!CheckParameterWithTypeOtherForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (false, count, message);
                }
            }

            #endregion

            #region SetParameterModel

            var (transferInfoFromModel, transferInfoToModel) = SetParameterModelForSetTransferInfo(bankFinancialTransferInfoDetail);

            #endregion

            if (bankFinancialTransferInfoDetail.TransferId == null)
            {
                using (var scope = new TransactionScope())
                {
                    var TransferId = GetTransferId(bankFinancialTransferInfoDetail.TransferType);
                    transferInfoFromModel.TransferId = TransferId;
                    transferInfoToModel.TransferId = TransferId;

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Minus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Create, transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Plus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Create, transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (InsertTransferReceipt(transferReceipt, TransferId) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Receipt Fail.");
                        }
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Insert Transfer Info And Insert Log Success.";
            }
            else
            {
                using (var scope = new TransactionScope())
                {
                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {

                        if (_bankFinancialService.UpdateTransferInfo(transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Info Minus Data Fail.");
                        };

                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Update, transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.UpdateTransferInfo(transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Info Plus Data Fail.");
                        };


                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Update, transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (InsertTransferReceipt(transferReceipt, bankFinancialTransferInfoDetail.TransferId) > 2)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Receipt Fail.");
                        }
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Update Transfer Info And Insert Log Success.";
            }

            return (true, count, message);
        }

        public (bool Status, int Count, string Message) DeleteTransferInfo(string transferId)
        {
            var count = 0;
            string message;

            if (transferId != null)
            {
                //Step1:Get Data
                var getData = _bankFinancialService.GetTransferInfoList(true,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             transferId);

                var transferInfoReceiptModel = _bankFinancialService.GetTransferInfoReceipt(transferId);

                BankFinancialTransferInfoTransDetailModel data;
                if (getData.Count() == 1)
                {
                    data = getData.FirstOrDefault();
                }
                else
                {
                    return (false, count, "Delete Transfer Info Data Fail.Data is not exist.");
                }

                //Step2: Set Data for Insert Log
                var (transferInfoFromModel, transferInfoToModel) = SetParameterModelForSetTransferInfo(data);

                using (var scope = new TransactionScope())
                {
                    var affectCount = 0;

                    //Step3:Insert Log
                    if (data.TransactionType == TransactionTypeEnum.PlusAndMinus
                       || data.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {
                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Delete, transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Minus Data Fail.");
                        }
                        affectCount++;
                    }
                    if (data.TransactionType == TransactionTypeEnum.PlusAndMinus
                       || data.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.InsertTransferInfoLog(ActionEnum.Delete, transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Log Plus Data Fail.");
                        }
                        affectCount++;
                    }

                    //Step4:Delete Data
                    if (_bankFinancialService.DeleteTransferInfo(transferId) != affectCount)
                    {
                        scope.Dispose();

                        return (false, count, "Delete Transfer Info Data Fail.");
                    }

                    if (transferInfoReceiptModel != null)
                    {
                        if (_bankFinancialService.DeleteTransferInfoReceipt(transferId) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Delete Transfer Info Receipt Fail.");
                        }
                    }

                    scope.Complete();

                    count = 1;
                }

                message = "Delete Transfer Info Success.";
            }
            else
            {
                return (false, count, "Delete Transfer Info Data Fail.TransferId is null.");
            }

            return (true, count, message);

        }

        #region Private Methods For Set Transfer Info

        /// <summary>
        /// 檢核 共有 資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckSharedParameterForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                            out string message)
        {
            message = string.Empty;

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
        private bool CheckParameterForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                      out string message)
        {
            message = string.Empty;

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
        private bool CheckParameterWithTypeOtherForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                                   out string message)
        {
            message = string.Empty;

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
        private (BankFinancialTransferInfoModel TransferInfoFromModel, BankFinancialTransferInfoModel TransferInfoToModel) SetParameterModelForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail)
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
                var xSize = 500m;
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

                var graphicOptions = new GraphicOptions
                {
                    CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed,
                    SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed
                };
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                var fileName = image.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                var fileBytes = ms.ToArray();

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

        #endregion

        #region Transfer Info Report

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReport(IEnumerable<int> siteIds,
                                                                                       IEnumerable<string> accountNos,
                                                                                       DateTime startDate,
                                                                                       DateTime endDate,
                                                                                       int pageNumber,
                                                                                       int pageSize,
                                                                                       out int totalPages,
                                                                                       out int rowCount)
        {
            return _bankFinancialService.GetTransferInfoReport(JsonConvert.SerializeObject(siteIds),
                                                               JsonConvert.SerializeObject(accountNos),
                                                               startDate,
                                                               endDate,
                                                               (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                                                               Math.Abs(pageSize),
                                                               out totalPages,
                                                               out rowCount);
        }

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReportForExport(IEnumerable<int> siteIds,
                                                                                  IEnumerable<string> accountNos,
                                                                                  DateTime startDate,
                                                                                  DateTime endDate)
        {
            return _bankFinancialService.GetTransferInfoReportForExport(JsonConvert.SerializeObject(siteIds),
                                                               JsonConvert.SerializeObject(accountNos),
                                                               startDate,
                                                               endDate);
        }

        #endregion

        #region Bank Financial Report

        public IEnumerable<BankFinancialDataModel> GetBankFinancialReport(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var keyValuePairs = new Dictionary<string, BankFinancialDataModel>();

            var accountData = _bankFinancialService.GetTransferAccountList();
            var payData = _paymentManageService.GetPayList();
            var siteData = _reportService.GetServerInfoList();
            var currencyData = _reportService.GetCurrencyList();

            var depositData = _bankFinancialService.GetDepositSummary(siteIds, currencyIds, startDate, endDate, timeZone);
            var withdrawalData = _bankFinancialService.GetWithdrawalSummary(siteIds, currencyIds, startDate, endDate, timeZone);
            var transferData = _bankFinancialService.GetTransferSummary(siteIds, currencyIds, startDate, endDate, timeZone);

            #region Data Combine

            foreach (var data in depositData)
            {
                BankFinancialDataModel bankFinancialDataModel;

                if (data.DepositType == DepositTypeEnum.LocalBanking)
                {
                    var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                    if (account != null)
                    {
                        if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                        {
                            bankFinancialDataModel = new BankFinancialDataModel();
                            keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                            bankFinancialDataModel.SiteId = data.SiteId;
                            bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                            bankFinancialDataModel.CurrencyId = data.CurrencyId;
                            bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                            bankFinancialDataModel.Channel = account.Channel;
                            bankFinancialDataModel.Agent = account.Agent;
                            bankFinancialDataModel.AccountNo = data.AccountNo;
                            bankFinancialDataModel.AccountName = account.DisplayName;
                            bankFinancialDataModel.Purpose = account.Purpose;
                            bankFinancialDataModel.AccountStatus = account.Status;
                        }

                        bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];
                        bankFinancialDataModel.DepositAmount += data.DepositSummary;
                    }
                }
                else
                {
                    var account = accountData.FirstOrDefault(x => x.PayId.ToString() == data.AccountNo && x.SiteId == data.SiteId && x.CurrencyId == data.CurrencyId);
                    var pay = payData.FirstOrDefault(x => x.Id.ToString() == data.AccountNo);

                    if (!keyValuePairs.ContainsKey($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        var status = pay.Status == 1 ? TransferAccountStatusEnum.Open : TransferAccountStatusEnum.Close;

                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = TransferChannelEnum.PSP;
                        bankFinancialDataModel.PayId = account?.PayId ?? pay.Id;
                        bankFinancialDataModel.PayName = account == null ? pay.PayName : account.PayName;
                        bankFinancialDataModel.AccountNo = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.AccountNo;
                        bankFinancialDataModel.AccountName = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.DisplayName;
                        bankFinancialDataModel.Purpose = account == null ? "1,2,3" : account.Purpose;
                        bankFinancialDataModel.AccountStatus = account?.Status ?? status;
                    }
                    bankFinancialDataModel = keyValuePairs[$"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];
                    bankFinancialDataModel.DepositAmount += data.DepositSummary;
                }

            }

            foreach (var data in withdrawalData)
            {
                BankFinancialDataModel bankFinancialDataModel;

                if (data.WithdrawalType == WithdrawalTypeEnum.LocalBank)
                {
                    var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                    if (account != null)
                    {
                        if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                        {
                            bankFinancialDataModel = new BankFinancialDataModel();
                            keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                            bankFinancialDataModel.SiteId = data.SiteId;
                            bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                            bankFinancialDataModel.CurrencyId = data.CurrencyId;
                            bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                            bankFinancialDataModel.Channel = account.Channel;
                            bankFinancialDataModel.Agent = account.Agent;
                            bankFinancialDataModel.AccountNo = data.AccountNo;
                            bankFinancialDataModel.AccountName = account.DisplayName;
                            bankFinancialDataModel.Purpose = account.Purpose;
                            bankFinancialDataModel.AccountStatus = account.Status;
                        }

                        bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                        bankFinancialDataModel.WithdrawalAmount += data.WithdrawalSummary;

                    }
                }
                else if (data.WithdrawalType == WithdrawalTypeEnum.OnlinePayment)
                {
                    var account = accountData.FirstOrDefault(x => x.PayId.ToString() == data.AccountNo && x.SiteId == data.SiteId && x.CurrencyId == data.CurrencyId);
                    var pay = payData.FirstOrDefault(x => x.Id.ToString() == data.AccountNo);

                    if (!keyValuePairs.ContainsKey($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        var status = pay.Status == 1 ? TransferAccountStatusEnum.Open : TransferAccountStatusEnum.Close;

                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = TransferChannelEnum.PSP;
                        bankFinancialDataModel.PayId = account?.PayId ?? pay.Id;
                        bankFinancialDataModel.PayName = account == null ? pay.PayName : account.PayName;
                        bankFinancialDataModel.AccountNo = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.AccountNo;
                        bankFinancialDataModel.AccountName = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.DisplayName;
                        bankFinancialDataModel.Purpose = account == null ? "1,2,3" : account.Purpose;
                        bankFinancialDataModel.AccountStatus = account?.Status ?? status;
                    }

                    bankFinancialDataModel = keyValuePairs[$"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                    bankFinancialDataModel.WithdrawalAmount += data.WithdrawalSummary;

                }

            }

            foreach (var data in transferData)
            {
                var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                if (account != null)
                {
                    BankFinancialDataModel bankFinancialDataModel;
                    if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = account.Channel;
                        bankFinancialDataModel.Agent = account.Agent;
                        bankFinancialDataModel.PayId = account.PayId;
                        bankFinancialDataModel.PayName = account.PayName;
                        bankFinancialDataModel.AccountNo = data.AccountNo;
                        bankFinancialDataModel.AccountName = account.DisplayName;
                        bankFinancialDataModel.Purpose = account.Purpose;
                        bankFinancialDataModel.AccountStatus = account.Status;
                    }

                    bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                    bankFinancialDataModel.TransferAmount += data.TransferSummary;
                }
            }

            #endregion

            #region 補0

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(value => value.SiteId)
                                                         .ThenBy(value => value.CurrencyId)
                                                         .ThenBy(value => value.Channel)
                                                         .ThenBy(value => value.AccountNo);

            return Data;
        }

        #endregion
    }
}
