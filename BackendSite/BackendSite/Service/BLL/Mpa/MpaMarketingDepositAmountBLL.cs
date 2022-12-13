using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

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
            IConfiguration configuration)
        {
            _depositAmountService = depositAmountService;
            _mapMpaCommonService = mapMpaCommonService;
            _cacheHelper = cacheHelper;
            _mpaDomainConfig = configuration.GetSection("MpaDomainConfig").Get<Dictionary<string, Dictionary<string, IEnumerable<string>>>>();
        }

        public async Task<DepositAmountSummaryResponse> GetDepositAmountSummaryAsync(DepositAmountRequest request)
        {
            var depositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(request.SiteId,
                request.StartDate, request.EndDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateAmountSummary(depositSummaryResult, request.StartDate, request.GetEndDate(), request.BaseCurrencyId);

            var compareDepositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(request.SiteId,
                request.CompareFromDate, request.CompareToDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateAmountSummary(compareDepositSummaryResult, request.CompareFromDate, request.GetCompareEndDate(), request.BaseCurrencyId);

            var response = new DepositAmountSummaryResponse
            {
                DepositAmount = depositSummaryResult.Sum(r => r.DepositAmount),
                CompareDepositAmount = compareDepositSummaryResult.Sum(r => r.DepositAmount)
            };
            response.ChangeRate = _mapMpaCommonService.GetPercent(response.DepositAmount, response.CompareDepositAmount);

            return response;
        }

        private async Task MultiplyExchangeRateAmountSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.DepositAmount = item.DepositAmount / rate;
            }
        }


        public async Task<DepositSinUpSummaryResponse> GetDepositSinUpSummaryAsync(DepositAmountRequest request)
        {
            var depositSignUpSummaryResult = (await _depositAmountService.GetDepositSignUpSummaryAsync(request.SiteId,
                request.StartDate, request.EndDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRateSinUpSummary(depositSignUpSummaryResult, request.StartDate, request.GetEndDate(), request.BaseCurrencyId);

            var sinSetting = GetSignUpSetting(request);

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
                    });
                }
            }

            foreach (var depositSignUp in depositSignUpSummaryResult)
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
                    Childs = mainSiteList.DistinctBy(x => x.SignUpUrl).Select(r => new SignUpChildDto
                    {
                        SignUpUrl = r.SignUpUrl,
                        FirstDepositAmount = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.FirstDepositAmount),
                        TotalDepositAmount = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).Sum(x => x.DepositAmount),
                        NoOfPlayers = mainSiteList.Where(x => x.SignUpUrl == r.SignUpUrl).DistinctBy(x => x.CustId)
                            .Count()
                    }).ToList()
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
                Childs = null
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
                Childs = null
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
                Childs = null
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
                Childs = landingPagesList.DistinctBy(x => x.SignUpUrl).Select(r => new SignUpChildDto
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
                Childs = null
            };
            response.SignUpData.Add(noneData);

            response.TotalSummary.FirstDepositAmount = response.SignUpData.Sum(r => r.FirstDepositAmount);
            response.TotalSummary.TotalDepositAmount = response.SignUpData.Sum(r => r.TotalDepositAmount);
            response.TotalSummary.NoOfPlayers = response.SignUpData.Sum(r => r.NoOfPlayers);

            #endregion

            return response;
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
                    config.CreditSiteName = " Bong88/Viva88";
                    config.MainSiteInfo = _mpaDomainConfig["Bwing"];
                    break;
            }

            return config;
        }

        private async Task MultiplyExchangeRateSinUpSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositSignUpBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.DepositAmount = item.DepositAmount / rate;
                if (item.FirstDepositAmount > 0)
                {
                    item.FirstDepositAmount = item.FirstDepositAmount / rate;
                }
            }
        }
    }
}
