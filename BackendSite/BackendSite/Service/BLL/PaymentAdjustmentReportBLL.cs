using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public IEnumerable<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport(string transId, int currency, string userName, int adjustType, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount, ref List<string> currencyAmount)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = paymentAdjustmentService.GetPaymentAdjustmentReport(
                    transId,
                    currency,
                    userName,
                    adjustType,
                    siteList?.Select(x => x.SiteId),
                    startDate,
                    endDate,
                    pageSize,
                    ref pageNumber,
                    ref totalPages,
                    ref rowCount,
                    ref currencyAmount
                );

            var result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
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

        public List<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentExportReport(string transId, int currency, string userName, int adjustType, DateTime startDate, DateTime endDate)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = paymentAdjustmentService.GetPaymentAdjustmentExportReport(
                    transId,
                    currency,
                    userName,
                    adjustType,
                    siteList?.Select(x => x.SiteId),
                    startDate,
                    endDate
                );

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

        public bool SetAdjustment(SetAdjustment input)
        {
            var reasons = _bankFinancialBll.GetReason("Finance/PaymentAdjustment");

            if (reasons == null || !reasons.Any())
            {
                _logger.LogError("FuncKey: Finance/PaymentAdjustment, GetReason has no data.");
                return false;
            }

            var m = Regex.Match(input.TransId, @"\d+");
            int.TryParse(m.Value, out var id);
            
            var beforeCategoryName = reasons.FirstOrDefault(x => x.ReasonId == input.BeforeCategory)?.Description;
            var afterCategoryName = reasons.FirstOrDefault(x => x.ReasonId == input.AfterCategory)?.Description;

            var logDetail = new Dictionary<string, string>
            {
                { "TransId", input.TransId },
                { "BeforeAdjustReason", beforeCategoryName },
                { "AfterAdjustReason", afterCategoryName },
                { "BeforeMemo", input.BeforeMemo },
                { "AfterMemo", input.AfterMemo }
            };

            var strLogDetail = JsonConvert.SerializeObject(logDetail);

            // 依 prefix 判斷更新哪張表
            if (input.TransId.StartsWith("a", StringComparison.OrdinalIgnoreCase))
                return paymentAdjustmentService.SetAdjustmentInfo(id, input.AfterCategory, input.AfterMemo, strLogDetail, input.Operator);

            return paymentAdjustmentService.SetLogAdminAdjustWallet(id, input.AfterCategory, input.AfterMemo, strLogDetail, input.Operator);
        }
    }
}
