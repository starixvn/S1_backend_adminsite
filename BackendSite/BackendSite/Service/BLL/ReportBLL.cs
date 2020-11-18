using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ReportBLL
    {
        private readonly ReportService reportService;
        private readonly SelLangBLL selLangBLL;

        public ReportBLL(ReportService reportService,
                         SelLangBLL selLangBLL)
        {
            this.reportService = reportService;
            this.selLangBLL = selLangBLL;
        }

        public IEnumerable<MainFinancialModel> GetMainFinancialReport(string siteIds,
                                                                      int currencyId,
                                                                      long startDate,
                                                                      long endDate)
        {
            Dictionary<string, MainFinancialModel> keyValuePairs = new Dictionary<string, MainFinancialModel>();
            string dateFormat = string.Empty;

            var reportList = reportService.GetMainFinancialReport(siteIds,
                                                                  currencyId,
                                                                  startDate,
                                                                  endDate);

            var currencyList = reportService.GetCurrencyList();
            ;
            #region Data Combine

            foreach (var data in reportList["Deposit"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Deposit = data.Summary;
            }

            foreach (var data in reportList["Withdrawal"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Withdrawal = data.Summary;

            }

            var adjustmentData = CaculateAdjstmentAmount(reportList["Adjustment"]);

            foreach (var data in adjustmentData)
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Adjustment = data.Summary;
            }

            foreach (var data in reportList["Bonus"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Bonus = data.Summary;
            }

            foreach (var data in reportList["Rebate"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Rebate = data.Summary;
            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(key => key.Date)
                                                        .ThenBy(key => key.CurrencyId);

            return Data;
        }

        /// <summary>
        /// Caculate Adjust
        /// </summary>
        /// <param name="partcialSummeries"></param>
        /// <returns></returns>
        private List<PartcialSummeryModel> CaculateAdjstmentAmount(IEnumerable<PartcialSummeryModel> partcialSummeries)
        {
            List<PartcialSummeryModel> result = new List<PartcialSummeryModel>();
            var dates = partcialSummeries.Select(x => x.Date).Distinct();
            var debitData = partcialSummeries.Where(x => x.AdjustType == AdjustType.Debit);
            var creditData = partcialSummeries.Where(x => x.AdjustType == AdjustType.Credit);
            int currencyId = partcialSummeries.Select(x => x.CurrencyId).FirstOrDefault();

            foreach (var date in dates)
            {
                PartcialSummeryModel partcialSummery = new PartcialSummeryModel();
                partcialSummery.Date = date;
                partcialSummery.CurrencyId = currencyId;
                partcialSummery.Summary = debitData.Where(x => x.Date == date).Select(x => x.Summary).Sum()
                                       - creditData.Where(x => x.Date == date).Select(x => x.Summary).Sum();

                result.Add(partcialSummery);

            }

            return result;
        }
    }
}
