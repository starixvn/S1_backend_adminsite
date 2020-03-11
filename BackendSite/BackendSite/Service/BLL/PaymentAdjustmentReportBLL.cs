using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class PaymentAdjustmentReportBLL
    {
        private readonly PaymentAdjustmentReportService paymentAdjustmentService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;

        public PaymentAdjustmentReportBLL(PaymentAdjustmentReportService paymentAdjustmentService, ServerInfoBLL serverInfoBLL, SelLangBLL selLangBLL)
        {
            this.paymentAdjustmentService = paymentAdjustmentService;
            this.serverInfoBLL = serverInfoBLL;
            this.selLangBLL = selLangBLL;
        }

        public IEnumerable<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport(string TransId, int Currency, string UserName, int AdjustType, long StartDate, long EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = paymentAdjustmentService.GetPaymentAdjustmentReport(
                    TransId, 
                    Currency, 
                    UserName,
                    AdjustType, 
                    siteList?.Select(x => x.SiteId), 
                    StartDate, 
                    EndDate, 
                    PageSize, 
                    ref PageNumber, 
                    ref TotalPages, 
                    ref RowCount
                );

            List<PaymentAdjustmentFrontEnd> result = new List<PaymentAdjustmentFrontEnd>();
            if(datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        CustId = row.CustId,
                        TransId = row.TransId,
                        CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}"),
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}")
                    });
                }
            }
            
            return result;
        }

        public IEnumerable<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentExportReport(string TransId, int Currency, string UserName, int AdjustType, long StartDate, long EndDate)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = paymentAdjustmentService.GetPaymentAdjustmentExportReport(
                    TransId,
                    Currency,
                    UserName,
                    AdjustType,
                    siteList?.Select(x => x.SiteId),
                    StartDate,
                    EndDate
                );

            List<PaymentAdjustmentFrontEnd> result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        TransId = row.TransId,
                        CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}"),
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}")
                    });
                }
            }

            return result;
        }
    }
}
