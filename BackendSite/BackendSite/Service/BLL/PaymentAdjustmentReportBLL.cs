using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class PaymentAdjustmentReportBLL
    {
        private readonly PaymentAdjustmentReportService paymentAdjustmentService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ILogger<PaymentAdjustmentReportBLL> _logger;

        public PaymentAdjustmentReportBLL(IServiceProvider provider)
        {
            this.paymentAdjustmentService = provider.GetRequiredService<PaymentAdjustmentReportService>();
            this.serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
            this.selLangBLL = provider.GetRequiredService<SelLangBLL>();
            this._bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
        }

        public IEnumerable<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string userName,
            IEnumerable<int> adjustTypes,
            DateTime startDate,
            DateTime endDate,
            string transId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount,
            out List<string> currencyAmount)
        {
            var siteList = serverInfoBLL.GetSiteList();
            var getFromAdjustmentInfo = false;
            var AdjustTypesForAI = new List<int>();
            var getFromLogAdminAdjustWallet = false;
            var AdjustTypesForLAAW = new List<int>();

            if (!siteIds.Any())
                siteIds = serverInfoBLL.GetSiteListByLoginUser().Select(x => x.SiteId);

            #region AdjustType

            if (!AdjustTypeCheck(adjustTypes,
            out getFromAdjustmentInfo,
            out AdjustTypesForAI,
            out getFromLogAdminAdjustWallet,
            out AdjustTypesForLAAW))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            var datas = paymentAdjustmentService.GetPaymentAdjustmentReport(
                    JsonConvert.SerializeObject(siteIds),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(AdjustTypesForAI),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(AdjustTypesForLAAW),
                    userName,
                    startDate,
                    endDate,
                    transId,
                    (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                    Math.Abs(pageSize),
                    out totalPages,
                    out rowCount,
                    out currencyAmount
                );

            var result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        SiteId = row.SiteId,
                        CurrencyId = row.CurrencyId,
                        CustId = row.CustId,
                        TransId = row.TransId,
                        CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        AdjustCategory = row.Category,
                        AdjustCategoryName = row.CategoryName,
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus ? selLangBLL.GetResourceValue($"lbl_AdjustmentStatus2") : selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}"),
                        Memo = row.Memo,
                        SabaID = row.SabaID,
                        Operator = row.Operator,
                        WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType
                    });
                }
            }

            return result;
        }

        public List<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentExportReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string userName,
            IEnumerable<int> adjustTypes,
            DateTime startDate,
            DateTime endDate,
            string transId)
        {
            var siteList = serverInfoBLL.GetSiteList();
            var getFromAdjustmentInfo = false;
            var AdjustTypesForAI = new List<int>();
            var getFromLogAdminAdjustWallet = false;
            var AdjustTypesForLAAW = new List<int>();

            if (!siteIds.Any())
                siteIds = serverInfoBLL.GetSiteListByLoginUser().Select(x => x.SiteId);

            #region AdjustType

            if (!AdjustTypeCheck(adjustTypes,
            out getFromAdjustmentInfo,
            out AdjustTypesForAI,
            out getFromLogAdminAdjustWallet,
            out AdjustTypesForLAAW))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            var datas = paymentAdjustmentService.GetPaymentAdjustmentExportReport(
                    JsonConvert.SerializeObject(siteIds),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(AdjustTypesForAI),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(AdjustTypesForLAAW),
                    userName,
                    startDate,
                    endDate,
                    transId);

            var result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        TransId = row.TransId,
                        CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        AdjustCategory = row.Category,
                        AdjustCategoryName = row.CategoryName,
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        SabaID = row.SabaID,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus ? selLangBLL.GetResourceValue($"lbl_AdjustmentStatus2") : selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}"),
                        Memo = row.Memo,
                        Operator = row.Operator,
                        WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType
                    });
                }
            }

            return result;
        }

        public IEnumerable<PaymentAdjustment> GetAdjustmentInfoByCust(int CustId,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount,
            ref List<string> CurrencyAmount)
        {
            return paymentAdjustmentService.GetAdjustmentInfoByCust(CustId, startDate, endDate, Math.Abs(pageSize), (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), out totalPages, out rowCount, ref CurrencyAmount);
        }

        public bool SetAdjustment(ParamsWithLog<SetAdjustment> input)
        {
            var reasons = _bankFinancialBll.GetReason("Finance/PaymentAdjustment");

            if (reasons == null || !reasons.Any())
            {
                _logger.LogError("FuncKey: Finance/PaymentAdjustment, GetReason has no data.");
                return false;
            }
            input.Before.CategoryName = reasons.FirstOrDefault(x => x.ReasonId == input.Before.Category)?.Description;
            input.After.CategoryName = reasons.FirstOrDefault(x => x.ReasonId == input.After.Category)?.Description;
            
            var m = Regex.Match(input.TransId, @"\d+");
            int.TryParse(m.Value, out var id);
            
            // 依 prefix 判斷更新哪張表
            if (input.TransId.StartsWith("a", StringComparison.OrdinalIgnoreCase))
                return paymentAdjustmentService.SetAdjustmentInfo(input, id);

            return paymentAdjustmentService.SetLogAdminAdjustWallet(input, id);
        }
        #region Private Methods

        private bool AdjustTypeCheck(IEnumerable<int> adjustTypes,
            out bool getFromAdjustmentInfo,
            out List<int> AdjustTypesForAI,
            out bool getFromLogAdminAdjustWallet,
            out List<int> AdjustTypesForLAAW)
        {
            getFromAdjustmentInfo = false;
            AdjustTypesForAI = new List<int>();
            getFromLogAdminAdjustWallet = false;
            AdjustTypesForLAAW = new List<int>();

            //先暫時寫死
            //原本是1234抓AdjustmentInfo,56抓LogAdminAdjustWallet
            //但是現在AdjustmentInfo也有56了......後續打算要怎麼規劃呢

            foreach (var adjustType in adjustTypes)
            {
                switch (adjustType)
                {
                    case 1:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(1);
                        break;
                    case 2:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(2);
                        break;
                    case 3:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(3);
                        break;
                    case 4:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(4);
                        break;
                    case 5:
                        getFromLogAdminAdjustWallet = true;
                        AdjustTypesForLAAW.Add(1);
                        break;
                    case 6:
                        getFromLogAdminAdjustWallet = true;
                        AdjustTypesForLAAW.Add(2);
                        break;
                }
            }
            return true;
        }

        #endregion

    }
}
