using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly Common _common;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public DepositReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _common = provider.GetRequiredService<Common>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
           
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        #endregion

        #region Deposit Report

        /// <summary>
        /// DepositReport/GetList 取得存款報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Deposit Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/DepositReport" })]
        public ApiResult GetList(GetListRequest request)
        {
            var data = _reportBll.GetDepositReport(request.CurrencyIds,
                                                   JsonConvert.SerializeObject(request.SiteIds),
                                                   JsonConvert.SerializeObject(request.CurrencyIds),
                                                   request.StartDate,
                                                   request.EndDate,
                                                   _common.TransferToTimeZoneFormat(request.TimeZone));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<DepositReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// DepositReport/Export 匯出存款報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Deposit Report > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/DepositReport" })]
        public IActionResult Export(GetListRequest request)
        {
            #region Get Data

            var exportData = _reportBll.GetDepositReport(
                request.CurrencyIds,
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate,
                request.EndDate,
                _common.TransferToTimeZoneFormat(request.TimeZone));

            var currencyData = _reportBll.GetCurrency();

            #endregion

            #region Initial Param

            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var summaryTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var summaryContent = new List<Dictionary<string, dynamic>>();
            var summaryTotalContent = new List<Dictionary<string, dynamic>>();

            var dateTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var dateTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var dateTotalTitle = new List<string>();
            var dateContent = new List<Dictionary<string, dynamic>>();
            var dateTotalContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Merge Title & Title & Total Title)

            #region Summary Merge Title

            var depositTypeTitles = new Dictionary<string, MergeTitleModel>();
            depositTypeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            depositTypeTitles.Add("OnlineBanking", new MergeTitleModel { Name = "Online Banking", MergeRange = (request.CurrencyIds.Count() + 1) * 2 });
            depositTypeTitles.Add("MobilePay", new MergeTitleModel { Name = "Mobile Pay", MergeRange = (request.CurrencyIds.Count() + 1) * 2 });
            depositTypeTitles.Add("LocalBanking", new MergeTitleModel { Name = "Local Banking", MergeRange = (request.CurrencyIds.Count() + 1) * 2 });
            depositTypeTitles.Add("TotalData", new MergeTitleModel { Name = "", MergeRange = (request.CurrencyIds.Count() + 1) * 6 });

            summaryTitlesToMerge.Add(depositTypeTitles);

            var mergeTitles = new Dictionary<string, MergeTitleModel>();
            mergeTitles.Add("SecondMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            mergeTitles.Add("OnlineAmount", new MergeTitleModel { Name = "Amount", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("OnlinePrecent", new MergeTitleModel { Name = "%", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("MobileAmount", new MergeTitleModel { Name = "Amount", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("MobilePrecent", new MergeTitleModel { Name = "%", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("LocalAmount", new MergeTitleModel { Name = "Amount", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("LocalPrecent", new MergeTitleModel { Name = "%", MergeRange = request.CurrencyIds.Count() + 1 });

            mergeTitles.Add("TotalAmount", new MergeTitleModel { Name = "Total Amount", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("TotalCount", new MergeTitleModel { Name = "Total Count", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("AverangeAmount", new MergeTitleModel { Name = "Averange Amount", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("SuccessRate", new MergeTitleModel { Name = "Success Rate(%)", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("FTDCount", new MergeTitleModel { Name = "FTD Count", MergeRange = request.CurrencyIds.Count() + 1 });
            mergeTitles.Add("AverangeProcessingTime", new MergeTitleModel { Name = "Averange Processing Time(D.HH:MM:SS)", MergeRange = request.CurrencyIds.Count() + 1 });

            summaryTitlesToMerge.Add(mergeTitles);

            #endregion

            #region Summary Title

            summaryTitles.Add("AccountName", new Api.ReportController.TitleModel { Name = "AccountName", SortId = 1 });
            summaryTitles.Add("OnlineAmount-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 200 });
            summaryTitles.Add("OnlinePrecent-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 400 });
            summaryTitles.Add("MobileAmount-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 600 });
            summaryTitles.Add("MobilePrecent-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 800 });
            summaryTitles.Add("LocalAmount-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 1000 });
            summaryTitles.Add("LocalPrecent-in-MYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 1200 });

            summaryTitles.Add("TotalAmount", new Api.ReportController.TitleModel { Name = "Total Amount In MYR", SortId = 1400 });
            summaryTitles.Add("TotalCount", new Api.ReportController.TitleModel { Name = "Total Count", SortId = 1600 });
            summaryTitles.Add("AverangeAmount", new Api.ReportController.TitleModel { Name = "Averange Amount In MYR", SortId = 1800 });
            summaryTitles.Add("SuccessRate", new Api.ReportController.TitleModel { Name = "Total Success Rate", SortId = 2000 });
            summaryTitles.Add("FTDCount", new Api.ReportController.TitleModel { Name = "Total FTD Count", SortId = 2200 });
            summaryTitles.Add("ApTime", new Api.ReportController.TitleModel { Name = "Total Averange Processing Time", SortId = 2400 });

            foreach (var curr in request.CurrencyIds)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var OnlineAmountPropName = $"OnlineAmount-{isoCode}";
                var OnlinePrecentPropName = $"OnlinePrecent-{isoCode}";
                var MobileAmountPropName = $"MobileAmount-{isoCode}";
                var MobilePrecentPropName = $"MobilePrecent-{isoCode}";
                var LocalAmountPropName = $"LocalAmount-{isoCode}";
                var LocalPrecentPropName = $"LocalPrecent-{isoCode}";

                var TotalAmountPropName = $"TotalAmount-{isoCode}";
                var TotalCountPropName = $"TotalCount-{isoCode}";
                var AverangeAmountPropName = $"AverangeAmount-{isoCode}";
                var SuccessRatePropName = $"SuccessRate-{isoCode}";
                var FTDCountPropName = $"FTDCount-{isoCode}";
                var ApTimePropName = $"ApTime-{isoCode}";

                var onlineAmountSortId = 100 + curr;
                var onlinePrecentSortId = 300 + curr;
                var mobileAmountSortId = 500 + curr;
                var mobilePrecentSortId = 700 + curr;
                var localAmountSortId = 900 + curr;
                var localPrecentSortId = 1100 + curr;

                var totalAmountSortId = 1300 + curr;
                var totalCountSortId = 1500 + curr;
                var averangeAmountSortId = 1700 + curr;
                var successRateSortId = 1900 + curr;
                var ftdCountSortId = 2100 + curr;
                var apTimeSortId = 2300 + curr;

                summaryTitles.Add(OnlineAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = onlineAmountSortId });
                summaryTitles.Add(OnlinePrecentPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = onlinePrecentSortId });
                summaryTitles.Add(MobileAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = mobileAmountSortId });
                summaryTitles.Add(MobilePrecentPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = mobilePrecentSortId });
                summaryTitles.Add(LocalAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = localAmountSortId });
                summaryTitles.Add(LocalPrecentPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = localPrecentSortId });

                summaryTitles.Add(TotalAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = totalAmountSortId });
                summaryTitles.Add(TotalCountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = totalCountSortId });
                summaryTitles.Add(AverangeAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = averangeAmountSortId });
                summaryTitles.Add(SuccessRatePropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = successRateSortId });
                summaryTitles.Add(FTDCountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = ftdCountSortId });
                summaryTitles.Add(ApTimePropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = apTimeSortId });

            }

            #endregion

            #region Summary Total Title

            var summaryTotalTitle = new List<string>
            {
                "OnlineAmount-in-MYR", "OnlinePrecent-in-MYR",
                "MobileAmount-in-MYR", "MobilePrecent-in-MYR",
                "LocalAmount-in-MYR","LocalPrecent-in-MYR",

                "TotalAmount","TotalCount",
                "AverangeAmount","SuccessRate",
                "FTDCount","ApTime"
            };

            #endregion

            #region Date Merge Title

            var accountNameTitles = new Dictionary<string, MergeTitleModel>();
            var dateMergeTitles = new Dictionary<string, MergeTitleModel>();

            accountNameTitles.Add("Account", new MergeTitleModel { Name = "Account", MergeRange = 1 });
            dateMergeTitles.Add("SecondMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            foreach (var data in exportData)
            {
                foreach (var account in data.SummaryAccountData)
                {
                    accountNameTitles.Add($"{account.AccountType}-{account.AccountId}-AccountName", new MergeTitleModel { Name = account.AccountName, MergeRange = (request.CurrencyIds.Count() + 1) * 5 });

                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-TotalAmount", new MergeTitleModel { Name = "Total Amount", MergeRange = request.CurrencyIds.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-TotalCount", new MergeTitleModel { Name = "Total Count", MergeRange = request.CurrencyIds.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount", new MergeTitleModel { Name = "Averange Amount", MergeRange = request.CurrencyIds.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-SuccessRate", new MergeTitleModel { Name = "Success Rate(%)", MergeRange = request.CurrencyIds.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-FTDCount", new MergeTitleModel { Name = "FTD Count", MergeRange = request.CurrencyIds.Count() + 1 });
                }
            }

            dateTitlesToMerge.Add(accountNameTitles);
            dateTitlesToMerge.Add(dateMergeTitles);

            #endregion

            #region Date Title & Date Total Title

            foreach (var data in exportData)
            {
                var i = 0;
                dateTitles.Add($"Date", new Api.ReportController.TitleModel { Name = "AccountName", SortId = 1 });

                foreach (var account in data.SummaryAccountData)
                {
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-TotalAmount", new Api.ReportController.TitleModel { Name = "Total Amount in MYR", SortId = i + 200 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-TotalCount", new Api.ReportController.TitleModel { Name = "Total Count", SortId = i + 400 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount", new Api.ReportController.TitleModel { Name = "Averange Amount in MYR", SortId = i + 600 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-SuccessRate", new Api.ReportController.TitleModel { Name = "Total Success Rate", SortId = i + 800 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-FTDCount", new Api.ReportController.TitleModel { Name = "Total FTD Count", SortId = i + 1000 });

                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-TotalAmount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-TotalCount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-SuccessRate");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-FTDCount");

                    foreach (var curr in request.CurrencyIds)
                    {
                        var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                        var TotalAmountPropName = $"{account.AccountType}-{account.AccountId}-TotalAmount-{isoCode}";
                        var TotalCountPropName = $"{account.AccountType}-{account.AccountId}-TotalCount-{isoCode}";
                        var AverangeAmountPropName = $"{account.AccountType}-{account.AccountId}-AverangeAmount-{isoCode}";
                        var SuccessRatePropName = $"{account.AccountType}-{account.AccountId}-SuccessRate-{isoCode}";
                        var FTDCountPropName = $"{account.AccountType}-{account.AccountId}-FTDCount-{isoCode}";

                        var totalAmountSortId = i + 100 + curr;
                        var totalCountSortId = i + 300 + curr;
                        var averangeAmountSortId = i + 500 + curr;
                        var successRateSortId = i + 700 + curr;
                        var ftdCountSortId = i + 900 + curr;

                        dateTitles.Add(TotalAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = totalAmountSortId });
                        dateTitles.Add(TotalCountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = totalCountSortId });
                        dateTitles.Add(AverangeAmountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = averangeAmountSortId });
                        dateTitles.Add(SuccessRatePropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = successRateSortId });
                        dateTitles.Add(FTDCountPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = ftdCountSortId });
                    }

                    i = i + 1000;
                }
            }

            #endregion

            #endregion

            #region Set Content

            foreach (var item in exportData)
            {
                //Summary Data Filling
                foreach (var accountItem in item.SummaryAccountData)
                {
                    foreach (var accountCurrencyItem in accountItem.SummaryData)
                    {
                        var OnlineAmountPropName = $"OnlineAmount-{accountCurrencyItem.IsoCode}";
                        var OnlinePrecentPropName = $"OnlinePrecent-{accountCurrencyItem.IsoCode}";
                        var MobileAmountPropName = $"MobileAmount-{accountCurrencyItem.IsoCode}";
                        var MobilePrecentPropName = $"MobilePrecent-{accountCurrencyItem.IsoCode}";
                        var LocalAmountPropName = $"LocalAmount-{accountCurrencyItem.IsoCode}";
                        var LocalPrecentPropName = $"LocalPrecent-{accountCurrencyItem.IsoCode}";

                        var TotalAmountPropName = $"TotalAmount-{accountCurrencyItem.IsoCode}";
                        var TotalCountPropName = $"TotalCount-{accountCurrencyItem.IsoCode}";
                        var AverangeAmountPropName = $"AverangeAmount-{accountCurrencyItem.IsoCode}";
                        var SuccessRatePropName = $"SuccessRate-{accountCurrencyItem.IsoCode}";
                        var FTDCountPropName = $"FTDCount-{accountCurrencyItem.IsoCode}";
                        var ApTimePropName = $"ApTime-{accountCurrencyItem.IsoCode}";

                        var getKey = $"{accountItem.AccountType}-{accountItem.AccountId}";

                        var getItem = summaryContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(OnlineAmountPropName, accountCurrencyItem.OnlineDeposit);
                            getItem.Add(OnlinePrecentPropName, accountCurrencyItem.OnlinePrecent);
                            getItem.Add(MobileAmountPropName, accountCurrencyItem.MobileDeposit);
                            getItem.Add(MobilePrecentPropName, accountCurrencyItem.MobilePrecent);
                            getItem.Add(LocalAmountPropName, accountCurrencyItem.LocalDeposit);
                            getItem.Add(LocalPrecentPropName, accountCurrencyItem.LocalPrecent);

                            getItem.Add(TotalAmountPropName, accountCurrencyItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountCurrencyItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountCurrencyItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountCurrencyItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountCurrencyItem.FTDCount);
                            getItem.Add(ApTimePropName, accountCurrencyItem.AverangeProcessingTime);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", $"{accountItem.AccountType}-{accountItem.AccountId}" },
                                { "AccountId", accountItem.AccountId },
                                { "AccountName", accountItem.AccountName },

                                { OnlineAmountPropName, accountCurrencyItem.OnlineDeposit },
                                { OnlinePrecentPropName, accountCurrencyItem.OnlinePrecent },
                                { MobileAmountPropName, accountCurrencyItem.MobileDeposit },
                                { MobilePrecentPropName, accountCurrencyItem.MobilePrecent },
                                { LocalAmountPropName, accountCurrencyItem.LocalDeposit },
                                { LocalPrecentPropName, accountCurrencyItem.LocalPrecent },

                                { TotalAmountPropName, accountCurrencyItem.TotalAmount },
                                { TotalCountPropName, accountCurrencyItem.TotalCount },
                                { AverangeAmountPropName, accountCurrencyItem.AverangeDeposit },
                                { SuccessRatePropName, accountCurrencyItem.SuccessRate },
                                { FTDCountPropName, accountCurrencyItem.FTDCount },
                                { ApTimePropName, accountCurrencyItem.AverangeProcessingTime },

                                { "OnlineAmount-in-MYR", accountItem.SummaryTotalData.OnlineDeposit },
                                { "OnlinePrecent-in-MYR", accountItem.SummaryTotalData.OnlinePrecent },
                                { "MobileAmount-in-MYR", accountItem.SummaryTotalData.MobileDeposit },
                                { "MobilePrecent-in-MYR", accountItem.SummaryTotalData.MobilePrecent },
                                { "LocalAmount-in-MYR", accountItem.SummaryTotalData.LocalDeposit },
                                { "LocalPrecent-in-MYR", accountItem.SummaryTotalData.LocalPrecent },

                                { "TotalAmount", accountItem.SummaryTotalData.TotalAmount },
                                { "TotalCount", accountItem.SummaryTotalData.TotalCount },
                                { "AverangeAmount", accountItem.SummaryTotalData.AverangeDeposit },
                                { "SuccessRate", accountItem.SummaryTotalData.SuccessRate },
                                { "FTDCount", accountItem.SummaryTotalData.FTDCount },
                                { "ApTime", accountItem.SummaryTotalData.AverangeProcessingTime }

                            };

                            summaryContent.Add(newItem);

                        }

                    }
                }

                //Summary Total Data Filling
                foreach (var currencyItem in item.SummaryCurrencyData)
                {
                    var OnlineAmountPropName = $"OnlineAmount-{currencyItem.IsoCode}";
                    var OnlinePrecentPropName = $"OnlinePrecent-{currencyItem.IsoCode}";
                    var MobileAmountPropName = $"MobileAmount-{currencyItem.IsoCode}";
                    var MobilePrecentPropName = $"MobilePrecent-{currencyItem.IsoCode}";
                    var LocalAmountPropName = $"LocalAmount-{currencyItem.IsoCode}";
                    var LocalPrecentPropName = $"LocalPrecent-{currencyItem.IsoCode}";

                    var TotalAmountPropName = $"TotalAmount-{currencyItem.IsoCode}";
                    var TotalCountPropName = $"TotalCount-{currencyItem.IsoCode}";
                    var AverangeAmountPropName = $"AverangeAmount-{currencyItem.IsoCode}";
                    var SuccessRatePropName = $"SuccessRate-{currencyItem.IsoCode}";
                    var FTDCountPropName = $"FTDCount-{currencyItem.IsoCode}";
                    var ApTimePropName = $"ApTime-{currencyItem.IsoCode}";

                    var getKey = "TOTAL";

                    var getItem = summaryContent.FirstOrDefault(x =>
                        x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                    );

                    if (getItem != null)
                    {
                        getItem.Add(OnlineAmountPropName, currencyItem.OnlineDeposit);
                        getItem.Add(OnlinePrecentPropName, currencyItem.OnlinePrecent);
                        getItem.Add(MobileAmountPropName, currencyItem.MobileDeposit);
                        getItem.Add(MobilePrecentPropName, currencyItem.MobilePrecent);
                        getItem.Add(LocalAmountPropName, currencyItem.LocalDeposit);
                        getItem.Add(LocalPrecentPropName, currencyItem.LocalPrecent);

                        getItem.Add(TotalAmountPropName, currencyItem.TotalAmount);
                        getItem.Add(TotalCountPropName, currencyItem.TotalCount);
                        getItem.Add(AverangeAmountPropName, currencyItem.AverangeDeposit);
                        getItem.Add(SuccessRatePropName, currencyItem.SuccessRate);
                        getItem.Add(FTDCountPropName, currencyItem.FTDCount);
                        getItem.Add(ApTimePropName, currencyItem.AverangeProcessingTime);

                    }
                    else
                    {
                        var newItem = new Dictionary<string, dynamic>()
                        {
                            { "Key", "TOTAL" },
                            { "AccountId", "" },
                            { "AccountName", "TOTAL：" },
                            { "CurrencyId", currencyItem.CurrencyId },
                            { "Currency", currencyItem.IsoCode },

                            { OnlineAmountPropName, currencyItem.OnlineDeposit },
                            { OnlinePrecentPropName, currencyItem.OnlinePrecent },
                            { MobileAmountPropName, currencyItem.MobileDeposit },
                            { MobilePrecentPropName, currencyItem.MobilePrecent },
                            { LocalAmountPropName, currencyItem.LocalDeposit },
                            { LocalPrecentPropName, currencyItem.LocalPrecent },

                            { TotalAmountPropName, currencyItem.TotalAmount },
                            { TotalCountPropName, currencyItem.TotalCount },
                            { AverangeAmountPropName, currencyItem.AverangeDeposit },
                            { SuccessRatePropName, currencyItem.SuccessRate },
                            { FTDCountPropName, currencyItem.FTDCount },
                            { ApTimePropName, currencyItem.AverangeProcessingTime },

                            { "OnlineAmount-in-MYR",  item.SummaryTotalData.OnlineDeposit },
                            { "OnlinePrecent-in-MYR", item.SummaryTotalData.OnlinePrecent },
                            { "MobileAmount-in-MYR", item.SummaryTotalData.MobileDeposit },
                            { "MobilePrecent-in-MYR", item.SummaryTotalData.MobilePrecent },
                            { "LocalAmount-in-MYR", item.SummaryTotalData.LocalDeposit },
                            { "LocalPrecent-in-MYR", item.SummaryTotalData.LocalPrecent },
                            { "TotalAmount", item.SummaryTotalData.TotalAmount },
                            { "TotalCount", item.SummaryTotalData.TotalCount },
                            { "AverangeAmount", item.SummaryTotalData.AverangeDeposit },
                            { "SuccessRate", item.SummaryTotalData.SuccessRate },
                            { "FTDCount", item.SummaryTotalData.FTDCount },
                            { "ApTime", item.SummaryTotalData.AverangeProcessingTime }
                        };

                        summaryTotalContent.Add(newItem);
                    }
                }

                //Date Data
                foreach (var accountItem in item.SummaryAccountData)
                {
                    //Date Data Filling
                    foreach (var accountDateItem in accountItem.DateData)
                    {
                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount-{accountDateItem.IsoCode}";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount-{accountDateItem.IsoCode}";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount-{accountDateItem.IsoCode}";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate-{accountDateItem.IsoCode}";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount-{accountDateItem.IsoCode}";

                        var getKey = $"{accountDateItem.Date}";

                        var getItem = dateContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(TotalAmountPropName, accountDateItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountDateItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountDateItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountDateItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountDateItem.FTDCount);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", $"{accountDateItem.Date}" },
                                { "Date", accountDateItem.Date },
                                { "AccountId", accountItem.AccountId },
                                { "AccountName", accountItem.AccountName },

                                { TotalAmountPropName, accountDateItem.TotalAmount },
                                { TotalCountPropName, accountDateItem.TotalCount },
                                { AverangeAmountPropName, accountDateItem.AverangeDeposit },
                                { SuccessRatePropName, accountDateItem.SuccessRate },
                                { FTDCountPropName, accountDateItem.FTDCount },
                            };

                            dateContent.Add(newItem);

                        }

                    }

                    foreach (var accountDateItem in accountItem.DateTotalData)
                    {
                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount";

                        var getKey = $"{accountDateItem.Date}";

                        var getItem = dateContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        getItem.Add(TotalAmountPropName, accountDateItem.TotalAmount);
                        getItem.Add(TotalCountPropName, accountDateItem.TotalCount);
                        getItem.Add(AverangeAmountPropName, accountDateItem.AverangeDeposit);
                        getItem.Add(SuccessRatePropName, accountDateItem.SuccessRate);
                        getItem.Add(FTDCountPropName, accountDateItem.FTDCount);

                    }

                    //Date Total Data Filling
                    foreach (var accountCurrencyItem in accountItem.SummaryData)
                    {

                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount-{accountCurrencyItem.IsoCode}";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount-{accountCurrencyItem.IsoCode}";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount-{accountCurrencyItem.IsoCode}";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate-{accountCurrencyItem.IsoCode}";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount-{accountCurrencyItem.IsoCode}";

                        var getKey = $"TOTAL";

                        var getItem = dateTotalContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(TotalAmountPropName, accountCurrencyItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountCurrencyItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountCurrencyItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountCurrencyItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountCurrencyItem.FTDCount);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", "TOTAL" },
                                { "Date", "TOTAL：" },

                                { TotalAmountPropName, accountCurrencyItem.TotalAmount },
                                { TotalCountPropName, accountCurrencyItem.TotalCount },
                                { AverangeAmountPropName, accountCurrencyItem.AverangeDeposit },
                                { SuccessRatePropName, accountCurrencyItem.SuccessRate },
                                { FTDCountPropName, accountCurrencyItem.FTDCount },

                            };

                            dateTotalContent.Add(newItem);

                        }

                    }

                    var getTotalKey = $"TOTAL";

                    var getTotalItem = dateTotalContent.FirstOrDefault(x =>
                        x.Any(y => y.Key == "Key" && (string)y.Value == getTotalKey)
                    );

                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount", accountItem.SummaryTotalData.TotalAmount);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount", accountItem.SummaryTotalData.TotalCount);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount", accountItem.SummaryTotalData.AverangeDeposit);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate", accountItem.SummaryTotalData.SuccessRate);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount", accountItem.SummaryTotalData.FTDCount);
                }
            }

            #endregion

            var exelParam = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "Summary",
                    Content = summaryContent,
                    TotalContent=summaryTotalContent,
                    Titles = summaryTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = summaryTotalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    IsSummary = true
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "ByDate",
                    Content = dateContent,
                    TotalContent=dateTotalContent,
                    Titles = dateTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = dateTotalTitle,
                    TitlesToMerge = dateTitlesToMerge,
                    IsSummary = true
                }
            };

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(exelParam);

            return _excelBll.ExcelFile(stream, "DepositReport");
        }

        #endregion
    }
}