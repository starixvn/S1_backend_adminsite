using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.GameStatisticalReport;
using BackendSite.Service.Model.Mpa.WinLoss;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Report;

using Newtonsoft.Json;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BackendSite.Service.Model.ApiModel;
using Namotion.Reflection;

namespace BackendSite.Service.BLL
{
    public class ReportBLL
    {
        #region Fields

        private readonly ReportService reportService;
        private readonly Common _common;
        private readonly BetListService _betListBll;
        private readonly IEnumerable<TpGameTypeOption> _tpGameTypeList;
        private readonly ICacheHelper _redisHelper;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly IMpaCommonService _mpaCommonService;
        private readonly IMpaCommonService _mapMpaCommonService;

        #endregion

        #region Constructors

        public ReportBLL(IServiceProvider provider)
        {
            reportService = provider.GetRequiredService<ReportService>();
            _common = provider.GetRequiredService<Common>();
            _betListBll = provider.GetRequiredService<BetListService>();
            _tpGameTypeList = provider.GetRequiredService<IEnumerable<TpGameTypeOption>>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _mpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            _mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
        }

        #endregion

        #region Library Formula

        /// <summary>
        /// 亞洲四捨五入
        /// </summary>
        /// <param name="number">判斷數字</param>
        /// <param name="point">小數點取位</param>
        /// <returns>四捨五入過後的數字</returns>
        private decimal RoundingNumber(decimal number, int point)
        {
            return Math.Round(number, point, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 數字轉換為百分比
        /// </summary>
        /// <param name="number">數字</param>
        /// <returns>百分比文字</returns>
        private string TransformToPercentage(decimal number, int point)
        {
            return Math.Round(number * 100, point, MidpointRounding.AwayFromZero).ToString() + "%";
        }

        #endregion

        #region Library Data

        public IEnumerable<ServerInfoModel> GetServer()
        {
            return reportService.GetServerInfoList();
        }

        public IEnumerable<CurrencyModel> GetCurrency()
        {
            return reportService.GetCurrencyList();
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await reportService.GetTpCategoryList();

            return tpCategories.Select(category => new TpCategoryBetLimit() { CategoryId = category.CategoryId, CategoryName = category.CategoryName }).ToList();
        }


        #endregion

        public IEnumerable<UserOnlineReportModel> GetUserOnlineReport(string siteIds, string selectDate, int timeZone)
        {
            var keyValuePairs = new Dictionary<string, UserOnlineReportModel>();
            string dateTimeFormat;
            var siteList = JsonConvert.DeserializeObject<List<int>>(siteIds);
            var deviceIdList = Enum.GetValues(typeof(DeviceId)).Cast<DeviceId>();
            var serverInfoList = reportService.GetServerInfoList();

            var date = new DateTimeOffset(Convert.ToDateTime(selectDate), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(date.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(date.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);
            var timeZoneString = _common.TransferToTimeZoneFormat(timeZone);

            var userOnlineList = reportService.GetUserOnlineReport(siteIds, startTime.DateTime, endTime.DateTime, timeZoneString);

            #region Data Statistical

            foreach (var data in userOnlineList)
            {
                dateTimeFormat = data.Time.ToString("HH:00");
                var userOnlineReportModel = new UserOnlineReportModel();
                keyValuePairs.Add($"{dateTimeFormat}_{data.SiteId}_{data.DeviceType}", userOnlineReportModel);
                userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{data.Time.AddHours(1).ToString("HH:00")}";
                userOnlineReportModel.SiteId = data.SiteId;
                userOnlineReportModel.DeviceType = data.DeviceType;
                userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                userOnlineReportModel.DeviceName = Enum.GetName(typeof(DeviceId), data.DeviceType);
                userOnlineReportModel.MaxCount = data.MaxCount;
            }

            #endregion

            #region 補0

            for (var i = 0; i < 24; i++)
            {
                foreach (var site in siteList)
                {
                    dateTimeFormat = Convert.ToDateTime(selectDate).AddHours(i).ToString("HH:00");

                    foreach (var id in deviceIdList)
                    {
                        if (!keyValuePairs.ContainsKey($"{dateTimeFormat}_{site}_{(short)id}"))
                        {
                            var userOnlineReportModel = new UserOnlineReportModel();
                            keyValuePairs.Add($"{dateTimeFormat}_{site}_{id}", userOnlineReportModel);
                            userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{Convert.ToDateTime(selectDate).AddHours(i + 1).ToString("HH:00")}";
                            userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                            userOnlineReportModel.SiteId = site;
                            userOnlineReportModel.DeviceType = (short)id;
                            userOnlineReportModel.DeviceName = Enum.GetName(typeof(DeviceId), id);
                        }
                    }
                }
            }

            #endregion

            // 不顯示Old登入次數
            var oldList = keyValuePairs.Where(x => x.Value.DeviceType == (short)DeviceId.Old);
            var isRemoveOld = oldList.All(x => x.Value.MaxCount == 0);
            if (isRemoveOld)
            {
                foreach (var item in oldList)
                {
                    keyValuePairs.Remove(item.Key);
                }
            }

            return keyValuePairs
                .Select(s => s.Value)
                .OrderBy(value => value.LocalTime)
                .ThenBy(value => value.SiteId);
        }
        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
            => reportService.GetTpFailRecordReport(input, out totalPages, out rowCount);

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
            => reportService.GetTpFailRecordReportForExport(input);

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReport(GetListInput input,
            out int totalPages,
            out int rowCount)
            => reportService.GetRewardBonusReport(input, out totalPages, out rowCount);

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReportExport(GetListInput input)
            => reportService.GetRewardBonusReportExport(input);

        #region Game Statistical Report

        public IEnumerable<GameStatisticalByCurrencyDto> GetGameStatisticalReport(GetListInputModel getListInput)
        {
            var key = $"GameStatisticalReport:{getListInput.SiteId}:{getListInput.StartDate}:{getListInput.EndDate}";
            var source = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => GetGameStatisticalSource(getListInput));
            return ProcessGameStatisticalSource(FilterSorce(getListInput.SiteId, source));
        }
        public Dictionary<string, IEnumerable<GameStatisticalDao>> ConvertToExport(IEnumerable<GameStatisticalByCurrencyDto> data)
        {
            var result = new Dictionary<string, IEnumerable<GameStatisticalDao>>();
            foreach (var d in data)
                result.Add(d.CurrencyName, d.CategoryDetail.SelectMany(y => y.GameDetail));

            return result;
        }
        
        private IEnumerable<GameStatisticalDao> GetGameStatisticalSource(GetListInputModel getListInput)
            => reportService.GetGameStatisticalReport(getListInput);

        private IEnumerable<GameStatisticalDao> FilterSorce(int siteId, IEnumerable<GameStatisticalDao> source)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2().First(x => x.SiteId == siteId);
            return source.Where(x => userServerInfo.ICurrencyList.Contains(x.CurrencyId));
        }

        private List<GameStatisticalByCurrencyDto> ProcessGameStatisticalSource(IEnumerable<GameStatisticalDao> source)
        {
            var userServerInfos = _serverInfoBll.GetSiteListByLoginUserV2();
            var currencyList = source.Select(t => new { CurrencyId = t.CurrencyId, CurrencyName = t.CurrencyName }).Distinct()
                .ToDictionary(t => t.CurrencyId, t => t.CurrencyName).OrderBy(x => x.Key);
            var categoryList = source.Select(t => new { CategoryId = t.CategoryId, CategoryName = t.CategoryName }).Distinct()
                .ToDictionary(t => t.CategoryId, t => t.CategoryName).OrderBy(x => x.Key);

            var result = new List<GameStatisticalByCurrencyDto>();

            foreach (var currency in currencyList)
            {
                var currencySummaryItem = new GameStatisticalByCurrencyDto() { CurrencyId = currency.Key, CurrencyName = currency.Value };
                var categorySummaryList = new List<GameStatisticalByCategoryDto>();
                foreach (var category in categoryList)
                {
                    var categorySummaryItem = new GameStatisticalByCategoryDto() { CategoryId = category.Key, CategoryName = category.Value };
                    var detail = source.Where(x => x.CurrencyId == currency.Key && x.CategoryId == category.Key).OrderByDescending(x => x.ValidBetSummary);

                    if (detail.Count() == 0) continue;
                    categorySummaryItem.GameDetail = detail;
                    categorySummaryList.Add(categorySummaryItem);
                }
                currencySummaryItem.CategoryDetail = categorySummaryList;
                result.Add(currencySummaryItem);
            }
            return result;
        }

        #endregion

        private decimal GetBaseCurrencyTotalAmount(IList<MpaRateDto> rateList,
            IEnumerable<IPaymentReportDetail> source,
            int baseCurrencyId)
        {
            var result = source.Sum(x =>
            {
                var rate = _mpaCommonService.MapperExchangeRate(rateList, x.FlowEndTime.UtcDateTime, (int)x.CurrencyId,
                    baseCurrencyId);
                var amount = x.Amount * rate;

                return amount;
            });

            return result;
        }

        private static DateTimeOffset GetEndDateForExchangeRate(DateTimeOffset endTime)
        {
            var now = DateTimeOffset.Now.ToUniversalTime();
            // 當月不需做處理
            if (endTime.ToString("yyyyMM") == now.ToString("yyyyMM"))
            {
                return endTime;
            }

            if (endTime > now)
            {
                return now;
            }

            // 該月最後一天
            var lastDate = new DateTime(endTime.Year, endTime.Month, DateTime.DaysInMonth(endTime.Year, endTime.Month));

            return new DateTimeOffset(lastDate, TimeSpan.Zero);
        }

        public async Task<IEnumerable<DepositPaymentReportSummary>> GetDepositPaymentReportAsync(
            GetPaymentReportParams input, TimeSpan offset, PaymentReportGroupByEnum groupBy)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)input).ConvertToMultiSiteParam(userServerInfo);
            var detailList = reportService.GetPaymentReportDepositDetail(input, JsonConvert.SerializeObject(multiSiteRequest))
                .ToList();

            if (groupBy is PaymentReportGroupByEnum.Date or PaymentReportGroupByEnum.Month)
            {
                detailList.ForEach(x =>
                {
                    x.CreateTime = x.CreateTime.ToOffset(offset);
                    x.FlowEndTime = x.FlowEndTime.ToOffset(offset);
                });
            }

            var rateList = (await _mpaCommonService.GetMpaExchangeRate(input.StartTime, GetEndDateForExchangeRate(input.EndTime))).ToList();
            var gData = detailList.GroupBy(x =>
            {
                return groupBy switch
                {
                    PaymentReportGroupByEnum.Currency => x.CurrencyId.ToString(),
                    PaymentReportGroupByEnum.Date => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM-dd"),
                    PaymentReportGroupByEnum.Month => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM"),
                    PaymentReportGroupByEnum.Gateway => x.PayName,
                    PaymentReportGroupByEnum.Type => x.DepositType.ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy,
                        $"Group by {groupBy.ToString()} not support")
                };
            });

            var result = gData.Select(g =>
            {
                var count = g.Count();
                var details = g
                    .GroupBy(x => new { x.CurrencyId, x.SiteName })
                    .Select(gg =>
                    {
                        var gCount = gg.Count();
                        var gTotalDepositAmount = gg.Sum(y => y.Amount);

                        return new DepositPaymentReportDetail
                        {
                            Key = gg.Key.CurrencyId.ToString(),
                            SiteName = gg.Key.SiteName,
                            TotalDepositCount = gCount,
                            AverageDepositAmount = decimal.Round(gTotalDepositAmount / gCount, 4),
                            FirstDepositCount = gg.Count(x => x.FirstDeposit),
                            TotalDepositAmount = decimal.Round(gTotalDepositAmount, 4),
                            AverageProcessingTime =
                                TimeSpan.FromMilliseconds(
                                        gg.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                                    .ToString(@"d\.hh\:mm\:ss")
                        };
                    });

                var totalDepositAmount =
                    GetBaseCurrencyTotalAmount(rateList, g.Select(x => x), input.BaseCurrencyId);

                return new DepositPaymentReportSummary
                {
                    Key = g.Key,
                    Children = details,
                    TotalDepositCount = count,
                    AverageDepositAmount = decimal.Round(totalDepositAmount / count, 4),
                    FirstDepositCount = g.Count(x => x.FirstDeposit),
                    TotalDepositAmount = decimal.Round(totalDepositAmount, 4),
                    AverageProcessingTime =
                        TimeSpan.FromMilliseconds(g.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                            .ToString(@"d\.hh\:mm\:ss")
                };
            });

            return result;
        }

        public async Task<IEnumerable<WithdrawalPaymentReportSummary>> GetWithdrawalPaymentReportAsync(
            GetPaymentReportParams input, TimeSpan offset, PaymentReportGroupByEnum groupBy)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)input).ConvertToMultiSiteParam(userServerInfo);
            var detailList = reportService.GetPaymentReportWithdrawalDetail(input, JsonConvert.SerializeObject(multiSiteRequest))
                .ToList();

            if (groupBy is PaymentReportGroupByEnum.Date or PaymentReportGroupByEnum.Month)
            {
                detailList.ForEach(x =>
                {
                    x.CreateTime = x.CreateTime.ToOffset(offset);
                    x.FlowEndTime = x.FlowEndTime.ToOffset(offset);
                });
            }

            var rateList = (await _mpaCommonService.GetMpaExchangeRate(input.StartTime, input.EndTime)).ToList();
            var gData = detailList.GroupBy(x =>
            {
                return groupBy switch
                {
                    PaymentReportGroupByEnum.Currency => x.CurrencyId.ToString(),
                    PaymentReportGroupByEnum.Date => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM-dd"),
                    PaymentReportGroupByEnum.Month => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM"),
                    PaymentReportGroupByEnum.Gateway => x.PayName,
                    PaymentReportGroupByEnum.Type => x.WithdrawalType.ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy,
                        $"Group by {groupBy.ToString()} not support")
                };
            });

            var result = gData.Select(g =>
            {
                var count = g.Count();
                var details = g
                    .GroupBy(x => new { x.CurrencyId, x.SiteName })
                    .Select(gg =>
                    {
                        var gCount = gg.Count();
                        var gTotalDepositAmount = gg.Sum(y => y.Amount);

                        return new WithdrawalPaymentReportDetail
                        {
                            Key = gg.Key.CurrencyId.ToString(),
                            SiteName = gg.Key.SiteName,
                            TotalWithdrawalCount = gCount,
                            AverageWithdrawalAmount = decimal.Round(gTotalDepositAmount / gCount, 4),
                            TotalWithdrawalAmount = decimal.Round(gTotalDepositAmount, 4),
                            AverageProcessingTime =
                                TimeSpan.FromMilliseconds(
                                        gg.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                                    .ToString(@"d\.hh\:mm\:ss")
                        };
                    });

                var totalDepositAmount =
                    GetBaseCurrencyTotalAmount(rateList, g.Select(x => x), input.BaseCurrencyId);

                return new WithdrawalPaymentReportSummary
                {
                    Key = g.Key,
                    Children = details,
                    TotalWithdrawalCount = count,
                    AverageWithdrawalAmount = decimal.Round(totalDepositAmount / count, 4),
                    TotalWithdrawalAmount = decimal.Round(totalDepositAmount, 4),
                    AverageProcessingTime =
                        TimeSpan.FromMilliseconds(g.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                            .ToString(@"d\.hh\:mm\:ss")
                };
            });

            return result;
        }

        public static byte[] ExportPaymentReport<TSummary, TDetail>(IEnumerable<TSummary> data)
            where TSummary : class, IPaymentReportSummary<TDetail>
            where TDetail : class
        {
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Report");

            sheet.CreateHeaders<TDetail>(freeze: true, index: 1);

            var gCellStyle = workbook.CreateCellStyle();
            gCellStyle.BorderBottom = BorderStyle.Medium;
            gCellStyle.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;
            gCellStyle.FillPattern = FillPattern.SolidForeground;

            var subCellStyle = workbook.CreateCellStyle();
            subCellStyle.BorderBottom = BorderStyle.Thin;
            subCellStyle.FillForegroundColor = IndexedColors.LightTurquoise.Index;
            subCellStyle.FillPattern = FillPattern.SolidForeground;

            var skipSProps = new[] { "Children" };
            var dProps = typeof(TDetail).GetProperties();
            var sProps = typeof(TSummary).GetProperties()
                .Where(x => !skipSProps.Contains(x.Name));
            var index = 1;
            foreach (var gData in data)
            {
                var groupRow = sheet.CreateRow(index);

                var idx = 0;
                foreach (var prop in sProps)
                {
                    var groupCell = groupRow.CreateCell(idx);
                    var val = typeof(TSummary).GetProperty(prop.Name)?.GetValue(gData);
                    groupRow.CreateCell(idx).SetCellValue(val?.ToString());
                    groupCell.CellStyle = gCellStyle;

                    idx++;
                }

                index++;

                var children = typeof(TSummary).GetProperty("Children")?.GetValue(gData) as IEnumerable<TDetail>;
                foreach (var detail in children)
                {
                    var row = sheet.CreateRow(index);

                    var i = 0;
                    foreach (var prop in dProps)
                    {
                        var subGroupCell = row.CreateCell(i);
                        var val = typeof(TDetail).GetProperty(prop.Name)?.GetValue(detail);
                        row.CreateCell(i).SetCellValue(val?.ToString());

                        if (i == 0)
                            subGroupCell.CellStyle = subCellStyle;

                        i++;
                    }
                    index++;
                }
            }

            sheet.AutoSizeColumn(0);

            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }
        
        #region Confiscated Summary Report

        public IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReportList(
            GetConfiscatedSummaryReportRequest request,
            int pageNumber,
            out IPageInfo pageInfo)
        {
            var dataList = GetConfiscatedSummaryReport(request, pageNumber , out pageInfo).ToList();
            
            if (request.TimeZone is not null)
            {
                TimeSpan.TryParseExact(request.TimeZone, @"\+hh\:mm", null, out var offset);
                foreach (var data in dataList)
                {
                    data.RegistrationDate += offset;
                    data.SuspendDate += offset;
                }
            }
            return dataList;
        }
        
        private IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReport(
            GetConfiscatedSummaryReportRequest request,
            int pageNumber,
            out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var key = $"ConfiscatedSummaryReport:{JsonConvert.SerializeObject(request.SiteIds)}:" +
                      $"{JsonConvert.SerializeObject(request.CurrencyIds)}:" +
                      $"{request.StartDate?.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{request.EndDate?.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{request.UserName}:" +
                      $"{request.BaseCurrencyId}";
            
            var cacheTime = TimeSpan.FromMinutes(2);
            if (_redisHelper.KeyExists(key))
            {
                //Export
                if (pageNumber == -1)
                {
                    return _redisHelper.GetListByRange<ConfiscatedSummaryReportDetail>(key, 0, pageNumber);
                }

                //Redis Pagination
                var total = (int) _redisHelper.GetListLength(key);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, pageNumber, request.PageSize);
                return _redisHelper.GetListByRange<ConfiscatedSummaryReportDetail>(key, startIndex, endIndex);
            }
            
            //Search range table
            var data = GetConfiscatedSummaryReportDetail(request).ToList();
            
            AddCache(data, key, cacheTime);

            //Export
            if (pageNumber == -1 || data.Count == 0)
            {
                return data;
            }

            var (index, count) = pageInfo.ListPagination(data.Count, pageNumber, request.PageSize);
            return data.GetRange(index, count);
        }
        
        public ConfiscatedSummary GetConfiscatedSummary(GetConfiscatedSummaryReportSummaryRequest request)
        {
            var getRequest = new GetConfiscatedSummaryReportRequest
            {
                SiteIds = request.SiteIds,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                UserName = request.UserName
            };
            var data = GetConfiscatedSummaryReport(getRequest, -1 , out _ ).ToList();

            var result = new ConfiscatedSummary();
            if (data.Any())
            {
                result = new ConfiscatedSummary
                {
                    TotalUser = data.Count,
                    TotalSeamlessWallet = data.Sum(x=>x.SeamlessWallet),
                    TotalDeposit = data.Sum(x=>x.TotalDeposit),
                    TotalWithdrawal = data.Sum(x=>x.TotalWithdrawal),
                    TotalNetDeposit = data.Sum(x=>x.NetDeposit),
                    TotalNetConfiscatedCash = data.Sum(x=>x.NetConfiscatedCash)
                };
            }
            
            return result;
        }
        
        private IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReportDetail(GetConfiscatedSummaryReportRequest request)
        {
            var dataList = reportService.GetConfiscatedSummaryReport(request).ToList();

            //抓3天內 避免MPA匯率未更新
            var rateStartTime = DateTime.UtcNow.AddDays(-3);
            var rateEndTime = DateTime.UtcNow;
            var rateList = _mapMpaCommonService.GetMpaExchangeRate(rateStartTime, rateEndTime).Result.ToList();
            
            foreach (var data in dataList)
            {
                var rate = rateList.Where(x=>x.CurrencyId == data.CurrencyId && x.TargetCurrencyId == request.BaseCurrencyId)
                    .MaxBy(item => item.RateDate).Rate;
                data.TotalDeposit *= rate;
                data.TotalWithdrawal *= rate;
                data.SeamlessWallet *= rate;
            }
            
            return dataList;
        }
        
        private void AddCache(IEnumerable<object> list, string masterKey, TimeSpan cacheTime)
        {
            _ = Task.Run(() =>
            {
                _redisHelper.InsertList(masterKey, cacheTime, list);
            });
        }
        
        #endregion
    }
}
