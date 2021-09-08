using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WinLostReportController : ControllerBase
    {
        #region Fields

        private readonly WinLostReportBLL _winLostReportBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public WinLostReportController(WinLostReportBLL winLostReportBLL,
                                       ExcelBLL excelBLL)
        {
            _winLostReportBLL = winLostReportBLL;
            _excelBLL = excelBLL;
        }

        #endregion

        #region API Model

        public class GetWinLostReportReq
        {
            public string StartDate { get; set; }

            public string EndDate { get; set; }

            public int Currency { get; set; }
        }

        #endregion

        #region Action

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "LicWinLost/Report" })]
        public ApiResult GetWinLostReport(GetWinLostReportReq request)
        {
            var data = _winLostReportBLL.GetWinLostReport(Convert.ToDateTime(request.StartDate),
                                                          Convert.ToDateTime(request.EndDate),
                                                          request.Currency,
                                                  false);

            if (data.Count() == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<WinLostReport>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [HttpPost]
        public IActionResult ExportReport(string StartDate,
                                          string EndDate,
                                          int Currency)
        {
            var Datas = _winLostReportBLL.GetWinLostReport(Convert.ToDateTime(StartDate),
                                                           Convert.ToDateTime(EndDate),
                                                           Currency,
                                                           true);

            var byDate = new List<Dictionary<string, dynamic>>();
            var Total = new List<Dictionary<string, dynamic>>();
            var byDateTitles = new Dictionary<string, int>();
            var TotalTitles = new Dictionary<string, int>();

            foreach (var item in Datas)
            {
                var gameStakePropName = $"Turnover-{item.GameName}";
                var gameWinlostPropName = $"Winlost-{item.GameName}";
                var gameStakeSortId = 100 + item.GameType;
                var gameWinlostSortId = 500 + item.GameType;

                var date = item.SettledDate.ToString("MM/dd/yyyy");

                var getByDateKey = $"{date}-{item.CurrencyId}";
                var getByDateItem = byDate.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getByDateKey)
                );

                if (getByDateItem != null && getByDateItem.TryGetValue("CurrencyId", out var currencyId) && (int)currencyId == item.CurrencyId)
                {
                    getByDateItem.TryGetValue("TurnoverAmount", out var stakeSummary);
                    getByDateItem["TurnoverAmount"] = (decimal)stakeSummary + item.StakeSummary;

                    getByDateItem.TryGetValue("WinlostAmount", out var WinlostAmount);
                    getByDateItem["WinlostAmount"] = (decimal)WinlostAmount + item.WinlostAmount;

                    if (!byDateTitles.ContainsKey(gameStakePropName))
                    {
                        byDateTitles.Add(gameStakePropName, gameStakeSortId);
                    }
                    if (getByDateItem.ContainsKey(gameStakePropName))
                    {
                        getByDateItem.TryGetValue(gameStakePropName, out var gameStake);
                        getByDateItem[gameStakePropName] = (decimal)gameStake + item.StakeSummary;
                    }
                    else
                    {
                        getByDateItem.Add(gameStakePropName, item.StakeSummary);
                    }

                    if (!byDateTitles.ContainsKey(gameWinlostPropName))
                    {
                        byDateTitles.Add(gameWinlostPropName, gameWinlostSortId);
                    }
                    if (getByDateItem.ContainsKey(gameWinlostPropName))
                    {
                        getByDateItem.TryGetValue(gameWinlostPropName, out var gameWinlost);
                        getByDateItem[gameWinlostPropName] = (decimal)gameWinlost + item.WinlostAmount;
                    }
                    else
                    {
                        getByDateItem.Add(gameWinlostPropName, item.WinlostAmount);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getByDateKey },
                        { "Date", date },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.CurrencyName },
                        { "GameType", item.GameType },
                        { "GameName", item.GameName },
                        { gameStakePropName, item.StakeSummary },
                        { gameWinlostPropName, item.WinlostAmount },
                        { "TurnoverAmount", item.StakeSummary },
                        { "WinlostAmount", item.WinlostAmount }
                    };

                    if (!byDateTitles.ContainsKey("Date") && !byDateTitles.ContainsKey("Currency"))
                    {
                        byDateTitles.Add("Date", 1);
                        byDateTitles.Add("Currency", 2);
                        byDateTitles.Add("TurnoverAmount", 3);
                        byDateTitles.Add("WinlostAmount", 4);
                    }

                    if (!byDateTitles.ContainsKey(gameStakePropName))
                    {
                        byDateTitles.Add(gameStakePropName, gameStakeSortId);
                    }
                    if (!byDateTitles.ContainsKey(gameWinlostPropName))
                    {
                        byDateTitles.Add(gameWinlostPropName, gameWinlostSortId);
                    }

                    byDate.Add(newItem);
                }

                var getByTotal = Total.FirstOrDefault(x => x.Any(y => y.Key == "CurrencyId" && (int)y.Value == item.CurrencyId));
                if (getByTotal != null)
                {
                    getByTotal.TryGetValue("TurnoverAmount", out var stakeSummary);
                    getByTotal["TurnoverAmount"] = (decimal)stakeSummary + item.StakeSummary;

                    getByTotal.TryGetValue("WinlostAmount", out var WinlostAmount);
                    getByTotal["WinlostAmount"] = (decimal)WinlostAmount + item.WinlostAmount;

                    if (!TotalTitles.ContainsKey(gameStakePropName))
                    {
                        TotalTitles.Add(gameStakePropName, gameStakeSortId);
                    }

                    if (getByTotal.ContainsKey(gameStakePropName))
                    {
                        getByTotal.TryGetValue(gameStakePropName, out var gameStake);
                        getByTotal[gameStakePropName] = (decimal)gameStake + item.StakeSummary;
                    }
                    else
                    {
                        getByTotal.Add(gameStakePropName, item.StakeSummary);
                    }

                    if (!TotalTitles.ContainsKey(gameWinlostPropName))
                    {
                        TotalTitles.Add(gameWinlostPropName, gameWinlostSortId);
                    }
                    if (getByTotal.ContainsKey(gameWinlostPropName))
                    {
                        getByTotal.TryGetValue(gameWinlostPropName, out var gameWinlost);
                        getByTotal[gameWinlostPropName] = (decimal)gameWinlost + item.WinlostAmount;
                    }
                    else
                    {
                        getByTotal.Add(gameWinlostPropName, item.WinlostAmount);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Date", date },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.CurrencyName },
                        { "GameType", item.GameType },
                        { "GameName", item.GameName },
                        { gameStakePropName, item.StakeSummary },
                        { gameWinlostPropName, item.WinlostAmount },
                        { "TurnoverAmount", item.StakeSummary },
                        { "WinlostAmount", item.WinlostAmount }
                    };

                    if (!TotalTitles.ContainsKey("Date") && !TotalTitles.ContainsKey("Currency"))
                    {
                        TotalTitles.Add("Date", 1);
                        TotalTitles.Add("Currency", 2);
                        TotalTitles.Add("TurnoverAmount", 3);
                        TotalTitles.Add("WinlostAmount", 4);
                    }

                    if (!TotalTitles.ContainsKey(gameStakePropName))
                    {
                        TotalTitles.Add(gameStakePropName, gameStakeSortId);
                    }
                    if (!TotalTitles.ContainsKey(gameWinlostPropName))
                    {
                        TotalTitles.Add(gameWinlostPropName, gameWinlostSortId);
                    }

                    Total.Add(newItem);
                }
            }

            var d = new List<ExportDataToExcel>()
            {
                new ExportDataToExcel(){SheetName="ByDate", Content = byDate, Titles=byDateTitles.OrderBy(x=> x.Value).Select(x=> x.Key).ToList()},
                new ExportDataToExcel(){SheetName="Total", Content = Total, Titles=TotalTitles.OrderBy(x=> x.Value).Select(x=> x.Key).ToList()},
            };

            var stream = _excelBLL.ExportToMultiSheet(d);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"WinlostReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

    }
}