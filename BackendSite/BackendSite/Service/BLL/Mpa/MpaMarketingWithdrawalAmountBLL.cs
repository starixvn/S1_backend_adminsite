using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.Mpa.Marketing;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Reflection;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingWithdrawalAmountBLL : IMpaMarketingWithdrawalAmountBLL
    {
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ICacheHelper _cacheHelper;

        public MpaMarketingWithdrawalAmountBLL(IMpaMarketingWithdrawalAmountService withdrawalAmountService,
            IMpaCommonService mapMpaCommonService,
            ICacheHelper cacheHelper)
        {
            _withdrawalAmountService = withdrawalAmountService;
            _mapMpaCommonService = mapMpaCommonService;
            _cacheHelper = cacheHelper;
        }

        public async Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request)
        {
            var withSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.StartDate,
                request.EndDate)).Where(x=> request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRate(withSummaryResult, request);

            var compareWithSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.CompareFromDate,
                request.CompareToDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();
            await MultiplyExchangeRate(compareWithSummaryResult, request);

            var response = new WithdrawalAmountSummaryResponse
            {
                WithdrawalAmount = withSummaryResult.Sum(x => x.WithdrawalAmount),
                CompareWithdrawalAmount = compareWithSummaryResult.Sum(x => x.WithdrawalAmount),
                WithdrawalUsers = withSummaryResult.DistinctBy(x => x.CustId).Count(),
                CompareWithdrawalUsers = compareWithSummaryResult.DistinctBy(x => x.CustId).Count()
            };
            response.WithdrawalAmountChangeRate = GetPercent(response.WithdrawalAmount, response.CompareWithdrawalAmount);
            response.WithdrawalUsersChangeRate = GetPercent(response.WithdrawalUsers, response.CompareWithdrawalUsers);

            return response;
        }

        public List<WithdrawalAmountDetailData> GetWithdrawalAmountDetail(WithdrawalAmountRequest request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var key = request.GetRedisKey();
            var masterKey = $"MpaWithdrawalAmountDetail:{key}";
            var sortKey = $"{masterKey}:{request.OrderByProperty}:{request.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(5);

            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (request.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<WithdrawalAmountDetailData>(masterKey, 0, -1).OrderBy(request);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, request.OrderBy);
                    }
                    var (sortIndex, sortCount) = pageInfo.ListPagination(sortData.Count, request.PageNumber, request.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (request.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<WithdrawalAmountDetailData>(masterKey, 0, request.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, request.PageNumber, request.PageSize);
                return _cacheHelper.GetListByRange<WithdrawalAmountDetailData>(masterKey, startIndex, endIndex);
            }

            var withDetails = _withdrawalAmountService.GetWithdrawalAmountDetailAsync(request.SiteId, request.StartDate,
                request.EndDate).Result.Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            MultiplyExchangeRate(withDetails, request).Wait();

            var currencyIdMapper = _mapMpaCommonService.GetCurrencyIsoCodeMapper().Result;
            var data = withDetails.GroupBy(x => new
            {
                x.UserName,
                x.CurrencyId,
                x.RegisteredDate,
            }).Select(r => new WithdrawalAmountDetailData
            {
                UserName = r.Key.UserName,
                CurrencyName = currencyIdMapper[r.Key.CurrencyId],
                RegisteredDate = RegisteredDateFormatter(r.Key.RegisteredDate),
                WithdrawalCount = r.Sum(x => x.WithdrawalCount),
                WithdrawalAmount = r.Sum(x => x.WithdrawalAmount),
            }).ToList();

            var list = request.OrderBy ? data.OrderBy(request) : data;
            AddCache(list, masterKey, sortKey, cacheTime, request.OrderBy);

            //Export
            if (request.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(data.Count, request.PageNumber, request.PageSize);
            return list.GetRange(index, count);
        }


        public byte[] ExportMpaWithdrawal(WithdrawalAmountSummaryResponse summaryData, 
            List<WithdrawalAmountDetailData> detailsData,
            bool isCompare, 
            string summaryTitle,
            string detailTitle,
            IEnumerable<PropertyInfo> properties = null)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(summaryData)); // summary sheets
            var ws2 = p.Workbook.Worksheets.Add(nameof(detailsData)); // detail sheets

            if (summaryData != null)
            {
                // 第一列
                var firstRow = ws.Cells[1, 1, 1, 4];
                firstRow.Merge = true;
                firstRow.Value = summaryTitle;
                firstRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCC99"));

                // 第二列
                var toCell = isCompare ? 4 : 2;
                var secondRow = ws.Cells[2, 1, 2, toCell];
                secondRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                secondRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                secondRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                ws.Cells[2, 1].Value = "";
                ws.Cells[2, 2].Value = "Report Date";
                ws.Cells[2, 2].EntireColumn.Width = 20;

                ws.Cells[3, 1].Value = "Total Withdrawal Amount";
                ws.Cells[3, 1].EntireColumn.Width = 30;
                ws.Cells[4, 1].Value = "Total Withdrawal Users";
                ws.Cells[4, 1].EntireColumn.Width = 30;

                if (isCompare)
                {
                    ws.Cells[2, 3].Value = "Compare Date";
                    ws.Cells[2, 3].EntireColumn.Width = 20;

                    ws.Cells[2, 4].Value = "% Change";
                    ws.Cells[2, 4].EntireColumn.Width = 20;

                    ws.Cells[3, 2].Value = summaryData.WithdrawalAmount;
                    ws.Cells[3, 3].Value = summaryData.CompareWithdrawalAmount;
                    ws.Cells[3, 4].Value = summaryData.WithdrawalAmountChangeRate;
                    ws.Cells[3, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[4, 2].Value = summaryData.WithdrawalUsers;
                    ws.Cells[4, 3].Value = summaryData.CompareWithdrawalUsers;
                    ws.Cells[4, 4].Value = summaryData.WithdrawalUsersChangeRate;
                    ws.Cells[4, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    ws.Cells[3, 2].Value = summaryData.WithdrawalAmount;
                    ws.Cells[4, 2].Value = summaryData.WithdrawalUsers;
                }
            }

            if (detailsData.Any())
            {
                properties ??= detailsData.FirstOrDefault()?.GetType().GetProperties();

                var rows = detailsData.Count() + 1;
                if (properties != null)
                {
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws2.Cells[2, 1].LoadFromCollection(detailsData, true);
                        ws2.Cells[2, 1].Value = "#";

                        // 第二列
                        var secondRow = ws2.Cells[1, 1, 2, cols];
                        secondRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        secondRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        secondRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                        // 第一列
                        var firstRow = ws2.Cells[1, 1, 1, cols];
                        firstRow.Value = detailTitle;
                        firstRow.Merge = true;
                        firstRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCC99"));

                        //設定欄位寬度
                        ws2.Cells[2, 1, rows, cols].EntireColumn.Width = 20;
                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }

        private async Task MultiplyExchangeRate<TEntity>(List<TEntity> source, WithdrawalAmountRequest request)
            where TEntity : class, IMpaMarketingWithdrawalAmountBaseDto
        {
            request.EndDate = request.EndDate.AddMonths(1).AddDays(-request.EndDate.AddMonths(1).Day);
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(request.StartDate, request.EndDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, request.BaseCurrencyId);
                item.WithdrawalAmount = item.WithdrawalAmount / rate;
            }
        }

        private static string GetPercent(decimal divisor, decimal dividend)
        {
            decimal percent;

            if (divisor < dividend)
            {
                percent = 1 - (divisor / dividend);
                percent *= -1;
            }
            else
            {
                percent = 1 - (dividend / divisor);
            }

            var result = percent.ToString("0%");
            return result;
        }

        private static string RegisteredDateFormatter(DateTime registeredDate)
        {
            var originRegisteredDate = registeredDate.ToString("dd-MM-yyyy hh:mm:ss");
            var originMonthNum = originRegisteredDate.Substring(3, 2);

            var monthText = originMonthNum switch
            {
                "01" => "Jan",
                "02" => "Feb",
                "03" => "Mar",
                "04" => "Apr",
                "05" => "May",
                "06" => "jun",
                "07" => "Jul",
                "08" => "Aug",
                "09" => "Sep",
                "10" => "Oct",
                "11" => "Nov",
                "12" => "Dec",
                _ => string.Empty
            };

            return $"{originRegisteredDate.Substring(0, 3)}{monthText}{originRegisteredDate.Substring(5, originRegisteredDate.Length - 5)}";
        }

        private void AddCache(List<WithdrawalAmountDetailData> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
        {
            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _cacheHelper.InsertList(masterKey, cacheTime, list);
                if (orderBy)
                {
                    _cacheHelper.StringSetAsync(sortKey, sortKey, cacheTime);
                }
            });
        }
    }
}
