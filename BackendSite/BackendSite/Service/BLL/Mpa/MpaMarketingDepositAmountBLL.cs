using System.Reflection;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingDepositAmountBLL : IMpaMarketingDepositAmountBLL
    {
        private readonly IMpaMarketingDepositAmountService _depositAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ICacheHelper _cacheHelper;
        private readonly Dictionary<string, Dictionary<string, IEnumerable<string>>> _mpaDomainConfig;

        public MpaMarketingDepositAmountBLL(IMpaMarketingDepositAmountService depositAmountService,
            IMpaCommonService mapMpaCommonService,
            ICacheHelper cacheHelper,
            ConfigBLL configBll)
        {
            _depositAmountService = depositAmountService;
            _mapMpaCommonService = mapMpaCommonService;
            _cacheHelper = cacheHelper;
            _mpaDomainConfig = configBll.GetConfigValuesBySiteName<Dictionary<string, IEnumerable<string>>>("MpaDomainConfig");
        }

        public async Task<DepositAmountSummaryResponse> GetDepositAmountSummaryAsync(DepositAmountRequest request)
        {
            var depositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(JsonConvert.SerializeObject(new List<int>() { request.SiteId }),
                request.StartDate.DateTime, request.EndDate.DateTime)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateAmountSummary(depositSummaryResult, request.StartDate, request.GetEndDate(), request.BaseCurrencyId);

            var compareDepositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(JsonConvert.SerializeObject(new List<int>() { request.SiteId }),
                request.StartDateCompared.DateTime, request.EndDateCompared.DateTime)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateAmountSummary(compareDepositSummaryResult, request.StartDateCompared, request.GetCompareEndDate(), request.BaseCurrencyId);

            var response = new DepositAmountSummaryResponse
            {
                DepositAmount = depositSummaryResult.Sum(r => r.DepositAmount),
                CompareDepositAmount = compareDepositSummaryResult.Sum(r => r.DepositAmount)
            };
            response.ChangeRate = _mapMpaCommonService.GetPercent(response.DepositAmount, response.CompareDepositAmount);

            return response;
        }

        public async Task MultiplyExchangeRateAmountSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.DepositAmount = item.DepositAmount * rate;
            }
        }


        public async Task<DepositSinUpSummaryResponse> GetDepositSinUpSummaryAsync(DepositAmountRequest request)
        {

            var sinSetting = GetSignUpSetting(request);

            var depositSignUpSummaryResult = (await _depositAmountService.GetDepositSignUpSummaryAsync(request.SiteId,
                request.StartDate.DateTime, request.EndDate.DateTime)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateSinUpSummary(depositSignUpSummaryResult, request.StartDate, request.GetEndDate(), request.BaseCurrencyId);

           // var sinSetting = GetSignUpSetting(request);

            #region Set SignUpGroupName 先把資料分群

            foreach (var mainSiteKey in sinSetting.MainSiteInfo.Keys)
            {
                foreach (var domain in sinSetting.MainSiteInfo[mainSiteKey].ToList())
                {
                    depositSignUpSummaryResult.ForEach(r =>
                    {
                        if (r.SignUpUrl == domain)
                        {
                            r.SignUpGroupName = mainSiteKey;
                        }

                        //For the case if member register time < 20/08/2020, also count in main group
                        if (request.SiteId == (int)SiteEnum.BWING && r.RegTime < new DateTime(2020, 08, 20))
                        {
                            r.SignUpGroupName = mainSiteKey;
                        }
                    });
                }
            }

            foreach (var depositSignUp in depositSignUpSummaryResult)
            {
                if (string.IsNullOrEmpty(depositSignUp.SignUpGroupName))
                {
                    //AFF
                    if (!string.IsNullOrEmpty(depositSignUp.AffCode))
                    {
                        depositSignUp.SignUpGroupName = "AFF";
                        continue;
                    }

                    //CreditSite
                    if (sinSetting.CreditSiteAgentIds.Contains(depositSignUp.AgentId))
                    {
                        depositSignUp.SignUpGroupName = sinSetting.CreditSiteName;
                        continue;
                    }

                    //APP
                    if (!string.IsNullOrEmpty(depositSignUp.SignUpUrl))
                    {
                        if (depositSignUp.SignUpUrl.Contains("app"))
                        {
                            depositSignUp.SignUpGroupName = "APP";
                        }
                    }

                    depositSignUp.SignUpGroupName =
                        string.IsNullOrEmpty(depositSignUp.SignUpUrl) ?
                            "None"
                            : "Landing Pages";
                }
            }

            #endregion

            #region Compose Data

            var response = new DepositSinUpSummaryResponse
            {
                SignUpData = new List<SignUpDatSumDto>(),
                TotalSummary = new SignUpTotalSummary()
            };

            foreach (var mainSiteKey in sinSetting.MainSiteInfo.Keys)
            {
                //MainSite 未來如果有多個 MainSite，改 Config 即可
                var mainSiteList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == mainSiteKey).ToList();
                var mainData = new SignUpDatSumDto
                {
                    SignUpSource = mainSiteKey,
                    SignUpUrl = mainSiteKey,
                    NoOfPlayers = mainSiteList.DistinctBy(x => x.CustId).Count(),
                    FirstDepositAmount = mainSiteList.Sum(r => r.FirstDepositAmount),
                    TotalDepositAmount = mainSiteList.Sum(r => r.DepositAmount),
                    Children = mainSiteList.Any()
                        ? mainSiteList.DistinctBy(x => x.SignUpUrl).Select(r => new SignUpChildDto
                        {
                            SignUpUrl = r.SignUpUrl,
                            FirstDepositAmount = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.FirstDepositAmount),
                            TotalDepositAmount = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.DepositAmount),
                            NoOfPlayers = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).DistinctBy(x => x.CustId)
                            .Count()
                        }).ToList()
                        : null
                };
                response.SignUpData.Add(mainData);
            }

            // AFF
            var affList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == "AFF").ToList();
            var affData = new SignUpDatSumDto
            {
                SignUpSource = "AFF",
                SignUpUrl = "AFF",
                NoOfPlayers = affList.DistinctBy(X => X.CustId).Count(),
                FirstDepositAmount = affList.Sum(r => r.FirstDepositAmount),
                TotalDepositAmount = affList.Sum(r => r.DepositAmount),
                Children = null
            };
            response.SignUpData.Add(affData);

            //APP
            var appList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == "APP").ToList();
            var appData = new SignUpDatSumDto
            {
                SignUpSource = "APP",
                SignUpUrl = "APP",
                NoOfPlayers = appList.DistinctBy(X => X.CustId).Count(),
                FirstDepositAmount = appList.Sum(r => r.FirstDepositAmount),
                TotalDepositAmount = appList.Sum(r => r.DepositAmount),
                Children = null
            };
            response.SignUpData.Add(appData);

            //CreditSite
            var creditSiteList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == sinSetting.CreditSiteName).ToList();
            var creditData = new SignUpDatSumDto
            {
                SignUpSource = sinSetting.CreditSiteName,
                SignUpUrl = sinSetting.CreditSiteName,
                NoOfPlayers = creditSiteList.DistinctBy(X => X.CustId).Count(),
                FirstDepositAmount = creditSiteList.Sum(r => r.FirstDepositAmount),
                TotalDepositAmount = creditSiteList.Sum(r => r.DepositAmount),
                Children = null
            };
            response.SignUpData.Add(creditData);

            //Landing Pages
            var debug = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == "Landing Pages").ToList();

            var landingPagesList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == "Landing Pages").ToList();
            var landingPagesData = new SignUpDatSumDto
            {
                SignUpSource = "Landing Pages",
                SignUpUrl = "Landing Pages",
                NoOfPlayers = landingPagesList.DistinctBy(X => X.CustId).Count(),
                FirstDepositAmount = landingPagesList.Sum(r => r.FirstDepositAmount),
                TotalDepositAmount = landingPagesList.Sum(r => r.DepositAmount),
                Children = landingPagesList.DistinctBy(x => x.SignUpUrl).Select(r => new SignUpChildDto
                {
                    SignUpUrl = r.SignUpUrl,
                    FirstDepositAmount = landingPagesList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.FirstDepositAmount),
                    TotalDepositAmount = landingPagesList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.DepositAmount),
                    NoOfPlayers = landingPagesList.Where(x => x.SignUpUrl == r.SignUpUrl).DistinctBy(x => x.CustId)
                        .Count()
                }).ToList()
            };
            response.SignUpData.Add(landingPagesData);

            //None
            var noneList = depositSignUpSummaryResult.Where(r => r.SignUpGroupName == "None").ToList();
            var noneData = new SignUpDatSumDto
            {
                SignUpSource = "None",
                SignUpUrl = "None",
                NoOfPlayers = noneList.DistinctBy(x => x.CustId).Count(),
                FirstDepositAmount = noneList.Sum(r => r.FirstDepositAmount),
                TotalDepositAmount = noneList.Sum(r => r.DepositAmount),
                Children = null
            };
            response.SignUpData.Add(noneData);

            response.TotalSummary.FirstDepositAmount = response.SignUpData.Sum(r => r.FirstDepositAmount);
            response.TotalSummary.TotalDepositAmount = response.SignUpData.Sum(r => r.TotalDepositAmount);
            response.TotalSummary.NoOfPlayers = response.SignUpData.Sum(r => r.NoOfPlayers);

            #endregion

            return response;
        }

        public async Task<List<SignUpChildDto>> GetDepositSinUpSummaryExportData(DepositAmountRequest request)
        {
            var data = await GetDepositSinUpSummaryAsync(request);
            var result = new List<SignUpChildDto>();

            foreach (var item in data.SignUpData)
            {
                if (item.Children != null)
                {
                    result.AddRange(item.Children);
                }
                else
                {
                    var child = new SignUpChildDto()
                    {
                        SignUpUrl = item.SignUpUrl,
                        FirstDepositAmount = item.FirstDepositAmount,
                        TotalDepositAmount = item.TotalDepositAmount,
                        NoOfPlayers = item.NoOfPlayers
                    };
                    result.Add(child);
                }
            }
            result.Add(new SignUpChildDto()
            {
                SignUpUrl = "Total",
                FirstDepositAmount = data.TotalSummary.FirstDepositAmount,
                TotalDepositAmount = data.TotalSummary.TotalDepositAmount,
                NoOfPlayers = data.TotalSummary.NoOfPlayers,
            });

            return result;
        }


        private SignUpConfig GetSignUpSetting(DepositAmountRequest request)
        {
            var config = new SignUpConfig();

            switch (request.SiteId)
            {
                case (int)SiteEnum.NOVA88:
                    config.AppDomain = "app.n17881788.com";
                    config.CreditSiteAgentIds = new List<int>() { 400, 417, 418, 419 }; //'Nova88Ref05','Nova88Ref06','Nova88Ref07','Nova88Ref08'
                    config.CreditSiteName = "Nova88.com";
                    config.MainSiteInfo = _mpaDomainConfig["Nova88"];
                    break;
                case (int)SiteEnum.GOAL123:
                    config.AppDomain = "app.goal123.com";
                    config.CreditSiteAgentIds = new List<int>() { 106734 }; //'Goal123Ref01'
                    config.CreditSiteName = "Bong88/Viva88";
                    config.MainSiteInfo = _mpaDomainConfig["Goal123"];
                    break;
                case (int)SiteEnum.BWING:
                    config.AppDomain = "app.bwing.com";
                    config.CreditSiteAgentIds = new List<int>() { 106733 }; //'BwingRef01'
                    config.CreditSiteName = "Bong88/Viva88";
                    config.MainSiteInfo = _mpaDomainConfig["Bwing"];
                    break;
            }

            return config;
        }

        public async Task MultiplyExchangeRateSinUpSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositSignUpBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.DepositAmount = item.DepositAmount * rate;
                if (item.FirstDepositAmount > 0)
                {
                    item.FirstDepositAmount = item.FirstDepositAmount * rate;
                }
            }
        }

        public IEnumerable<DepositSignUpDetailData> GetDepositSignUpDetail(DepositAmountRequest request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var key = request.GetRedisKey();
            var masterKey = $"MpaDepositSignUpDetail:{key}";
            var sortKey = $"{masterKey}:{request.OrderByProperty}:{request.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(5);

            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (request.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<DepositSignUpDetailData>(masterKey, 0, -1).OrderBy(request);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, request.OrderBy);
                    }
                    var (sortIndex, sortCount) = pageInfo.ListPagination(sortData.Count(), request.PageNumber, request.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (request.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<DepositSignUpDetailData>(masterKey, 0, request.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, request.PageNumber, request.PageSize);
                return _cacheHelper.GetListByRange<DepositSignUpDetailData>(masterKey, startIndex, endIndex);
            }


            var depositSignUpDetail = _depositAmountService.GetDepositSignUpDetail(
                JsonConvert.SerializeObject(new List<int>() { request.SiteId }), request.StartDate.DateTime, request.EndDate.DateTime)
                .Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            MultiplyExchangeRateSinUpSummary(depositSignUpDetail, request.StartDate, request.GetEndDate(), request.BaseCurrencyId).Wait();

            var currencyIdMapper = _mapMpaCommonService.GetCurrencyIsoCodeMapper().Result;

            var data = depositSignUpDetail.GroupBy(x => new
            {
                x.UserName,
                x.SignUpUrl,
                x.AffCode,
                x.CurrencyId,
                x.RegisteredDate,
            }).Select(r => new DepositSignUpDetailData
            {
                UserName = r.Key.UserName,
                CurrencyName = currencyIdMapper[r.Key.CurrencyId],
                RegisteredDate = _mapMpaCommonService.RegisteredDateFormatter(r.Key.RegisteredDate),
                SignUpUrl = r.Key.SignUpUrl,
                AffCode = r.Key.AffCode,
                DepositCount = r.Sum(x => x.DepositCount),
                TotalDepositAmount = r.Sum(x => x.DepositAmount),
                FirstDepositAmount = r.Sum(x => x.FirstDepositAmount),
            }).ToList();

            var list = request.OrderBy ? data.OrderBy(request).ToList() : data;
            AddCache(list, masterKey, sortKey, cacheTime, request.OrderBy);

            //Export
            if (request.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(data.Count, request.PageNumber, request.PageSize);
            return list.GetRange(index, count);

        }

        private void AddCache(IEnumerable<DepositSignUpDetailData> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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

        public byte[] ExportMpaDeposit(DepositAmountSummaryResponse depositAmountSummary,
            List<SignUpChildDto> depositSinUpSummary,
            List<DepositSignUpDetailData> depositSignUpDetail,
            DepositAmountRequest request)
        {
            var currencyIdMapper = _mapMpaCommonService.GetCurrencyIsoCodeMapper().Result;
            var title = $"Withdrawal Amount, {request.StartDate.ToString("yyyy-dd-MM")} - {request.EndDate.ToString("yyyy-dd-MM")}, View in {currencyIdMapper[request.BaseCurrencyId]}";
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(depositAmountSummary)); // AmountSummary
            var ws2 = p.Workbook.Worksheets.Add(nameof(depositSinUpSummary)); // SinUpSummary
            var ws3 = p.Workbook.Worksheets.Add(nameof(depositSignUpDetail)); // detail

            if (depositAmountSummary != null)
            {
                // 第一列
                var firstRow = ws.Cells[1, 1, 1, 4];
                firstRow.Merge = true;
                firstRow.Value = title;
                firstRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCC99"));

                // 第二列
                var toCell = request.IsCompared ? 4 : 2;
                var secondRow = ws.Cells[2, 1, 2, toCell];
                secondRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                secondRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                secondRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                ws.Cells[2, 1].Value = "Total";
                ws.Cells[2, 1].EntireColumn.Width = 20;

                ws.Cells[2, 2].Value = "Report Date";
                ws.Cells[2, 2].EntireColumn.Width = 20;

                ws.Cells[3, 1].Value = "TDeposit Amount";
                ws.Cells[3, 1].EntireColumn.Width = 30;

                ws.Cells[3, 2].Value = depositAmountSummary.DepositAmount;
                ws.Cells[3, 2].EntireColumn.Width = 30;

                if (request.IsCompared)
                {
                    ws.Cells[2, 3].Value = "Compare Date";
                    ws.Cells[2, 3].EntireColumn.Width = 20;

                    ws.Cells[2, 4].Value = "% Change";
                    ws.Cells[2, 4].EntireColumn.Width = 20;

                    ws.Cells[3, 3].Value = depositAmountSummary.CompareDepositAmount;
                    ws.Cells[3, 4].Value = depositAmountSummary.ChangeRate;
                    ws.Cells[3, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
            }

            if (depositSinUpSummary.Any())
            {
                IEnumerable<PropertyInfo> depositSinUpSummaryProperties = null;
                depositSinUpSummaryProperties = depositSinUpSummary.FirstOrDefault()?.GetType().GetProperties();

                var rows = depositSinUpSummary.Count() + 1;
                if (depositSinUpSummaryProperties != null)
                {
                    var cols = depositSinUpSummaryProperties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws2.Cells[2, 1].LoadFromCollection(depositSinUpSummary, true);

                        // 第二列
                        var secondRow = ws2.Cells[1, 1, 2, cols];
                        secondRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        secondRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        secondRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                        // 第一列
                        var firstRow = ws2.Cells[1, 1, 1, cols];
                        firstRow.Value = title;
                        firstRow.Merge = true;
                        firstRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCC99"));

                        //設定欄位寬度
                        ws2.Cells[2, 1, rows, cols].EntireColumn.Width = 20;
                    }
                }
            }

            if (depositSignUpDetail.Any())
            {
                IEnumerable<PropertyInfo> depositSignUpDetailProperties = null;
                depositSignUpDetailProperties = depositSignUpDetail.FirstOrDefault()?.GetType().GetProperties();

                var rows = depositSignUpDetail.Count() + 1;
                if (depositSignUpDetailProperties != null)
                {
                    var cols = depositSignUpDetailProperties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws3.Cells[2, 1].LoadFromCollection(depositSignUpDetail, true);
                        ws3.Cells[2, 1].Value = "#";

                        // 第二列
                        var secondRow = ws3.Cells[1, 1, 2, cols];
                        secondRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        secondRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        secondRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                        // 第一列
                        var firstRow = ws3.Cells[1, 1, 1, cols];
                        firstRow.Value = title;
                        firstRow.Merge = true;
                        firstRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCC99"));

                        //設定欄位寬度
                        ws3.Cells[2, 1, rows, cols].EntireColumn.Width = 20;
                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }
    }
}
