using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MpaNRUBLL
    {
        private readonly NRUService _nruService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ICacheHelper _redisHelper;
        private readonly Dictionary<string, Dictionary<string, IEnumerable<string>>> _mpaDomainConfig;

        public MpaNRUBLL(IServiceProvider provider)
        {
            _nruService = provider.GetRequiredService<NRUService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _mpaDomainConfig = configBll.GetConfigValuesBySiteName<Dictionary<string, IEnumerable<string>>>("MpaDomainConfig");
        }

        #region MpaMarketingNRU

        public NRUReportData GetMpaMarketingNRU(MpaNRUReportController.GetMarketingNRUDataRequest input)
        {
            var key = $"MpaMarketingNRU:{input.SiteId}:" +
                      $"{JsonConvert.SerializeObject(input.CurrencyIds)}:" +
                      $"{input.StartDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.EndDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.BaseCurrencyId}";

            var data = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => _nruService.GetMpaMarketingNRUInfo(
                    input.SiteId,
                    JsonConvert.SerializeObject(input.CurrencyIds),
                    input.StartDate.DateTime,
                    input.EndDate.DateTime,
                    input.BaseCurrencyId));

            #region Init

            Dictionary<string, IEnumerable<string>> groupUrlDic;
            var landingPagesList = data;

            #endregion

            var nruData = new NRUReportData
            {
                SummaryData = new SummaryData
                {
                    NewlyRegisterUsers = data.Count
                },
                TotalSignUpData = new SignUpData
                {
                    SignUpSource = null,
                    NewlyRegisterUsers = data.Count,
                    DepositUsers = data.Count(info => info.TotalDeposit > 0),
                    FirstDepositAmount = data.Sum(info => info.FirstDeposit),
                    TotalDepositAmount = data.Sum(info => info.TotalDeposit),
                },
                SignUpDataSum = new List<SignUpDataSum>()
            };

            switch (input.SiteId)
            {
                case 1:

                    #region NOVA88

                    //AFF
                    var affList = landingPagesList.Where(x => !string.IsNullOrEmpty(x.AffCode)).ToList();
                    var affData = new SignUpDataSum
                    {
                        SignUpSource = "AFF",
                        NewlyRegisterUsers = affList.Count(),
                        DepositUsers = affList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = affList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = affList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(affList).ToList();
                    nruData.SignUpDataSum.Add(affData);

                    //Nova88.com
                    var creditSite = new List<int>() { 400, 417, 418, 419 }; //'Nova88Ref05','Nova88Ref06','Nova88Ref07','Nova88Ref08'
                    var nova88ComList = landingPagesList.Where(x => creditSite.Contains(x.AgentId)).ToList();
                    var nova88ComData = new SignUpDataSum
                    {
                        SignUpSource = "Nova88.com",
                        NewlyRegisterUsers = nova88ComList.Count(),
                        DepositUsers = nova88ComList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = nova88ComList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = nova88ComList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(nova88ComList).ToList();
                    nruData.SignUpDataSum.Add(nova88ComData);

                    //GroupBy Domain
                    groupUrlDic = _mpaDomainConfig["Nova88"];

                    foreach (var groupUrl in groupUrlDic.Keys)
                    {
                        var nova88Data = new SignUpDataSum
                        {
                            Children = new List<SignUpData>(),
                            SignUpSource = groupUrl
                        };

                        var domainList = _mpaDomainConfig["Nova88"][groupUrl];
                        foreach (var domain in domainList)
                        {
                            var signUpData = landingPagesList.Where(x => x.SignupUrl?.Contains(domain) ?? false).ToList();
                            if (signUpData.Count > 0)
                            {
                                landingPagesList = landingPagesList.Except(signUpData).ToList();
                                var childData = new SignUpData
                                {
                                    SignUpSource = domain,
                                    NewlyRegisterUsers = signUpData.Count(),
                                    DepositUsers = signUpData.Count(info => info.TotalDeposit > 0),
                                    FirstDepositAmount = signUpData.Sum(info => info.FirstDeposit),
                                    TotalDepositAmount = signUpData.Sum(info => info.TotalDeposit)
                                };
                                nova88Data.Children.Add(childData);
                            }
                        }

                        nova88Data.NewlyRegisterUsers = nova88Data.Children.Sum(info => info.NewlyRegisterUsers);
                        nova88Data.DepositUsers = nova88Data.Children.Sum(info => info.DepositUsers);
                        nova88Data.FirstDepositAmount = nova88Data.Children.Sum(info => info.FirstDepositAmount);
                        nova88Data.TotalDepositAmount = nova88Data.Children.Sum(info => info.TotalDepositAmount);
                        nruData.SignUpDataSum.Add(nova88Data);
                    }

                    //APP
                    var appList = landingPagesList.Where(x => x.SignupUrl?.Contains("app") ?? false).ToList();
                    var appData = new SignUpDataSum
                    {
                        SignUpSource = "APP",
                        NewlyRegisterUsers = appList.Count(),
                        DepositUsers = appList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = appList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = appList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(appList).ToList();
                    nruData.SignUpDataSum.Add(appData);

                    //Landing Pages
                    var landingPagesChilds = landingPagesList
                        .GroupBy(i => i.SignupUrl)
                        .Select(i => new SignUpData
                        {
                            SignUpSource = i.First().SignupUrl,
                            NewlyRegisterUsers = i.Count(),
                            DepositUsers = i.Count(info => info.TotalDeposit > 0),
                            FirstDepositAmount = i.Sum(info => info.FirstDeposit),
                            TotalDepositAmount = i.Sum(info => info.TotalDeposit),
                        }).ToList();

                    var landingPagesData = new SignUpDataSum
                    {
                        Children = landingPagesChilds,
                        SignUpSource = "Landing Pages",
                        NewlyRegisterUsers = landingPagesList.Count(),
                        DepositUsers = landingPagesList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = landingPagesList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = landingPagesList.Sum(info => info.TotalDeposit),
                    };
                    nruData.SignUpDataSum.Add(landingPagesData);

                    #endregion

                    break;

                case 2:

                    #region GOAL123

                    //AFF
                    var goalAffList = landingPagesList.Where(x => !string.IsNullOrEmpty(x.AffCode)).ToList();
                    var goalAffData = new SignUpDataSum
                    {
                        SignUpSource = "AFF",
                        NewlyRegisterUsers = goalAffList.Count,
                        DepositUsers = goalAffList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = goalAffList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = goalAffList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(goalAffList).ToList();
                    nruData.SignUpDataSum.Add(goalAffData);

                    //Bong88/Viva88
                    var goalCreditSite = new List<int>() { 106734 }; //'Goal123Ref01'
                    var bong88List = landingPagesList.Where(x => goalCreditSite.Contains(x.AgentId)).ToList();
                    var bong88ListData = new SignUpDataSum
                    {
                        SignUpSource = "Bong88/Viva88",
                        NewlyRegisterUsers = bong88List.Count(),
                        DepositUsers = bong88List.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = bong88List.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = bong88List.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(bong88List).ToList();
                    nruData.SignUpDataSum.Add(bong88ListData);

                    //GroupBy Domain
                    groupUrlDic = _mpaDomainConfig["Goal123"];

                    foreach (var groupUrl in groupUrlDic.Keys)
                    {
                        var goal123Data = new SignUpDataSum
                        {
                            Children = new List<SignUpData>(),
                            SignUpSource = groupUrl
                        };

                        var domainList = _mpaDomainConfig["Goal123"][groupUrl];
                        foreach (var domain in domainList)
                        {
                            var signUpData = landingPagesList.Where(x => x.SignupUrl?.Contains(domain) ?? false).ToList();
                            if (signUpData.Count > 0)
                            {
                                landingPagesList = landingPagesList.Except(signUpData).ToList();
                                var childData = new SignUpData
                                {
                                    SignUpSource = domain,
                                    NewlyRegisterUsers = signUpData.Count(),
                                    DepositUsers = signUpData.Count(info => info.TotalDeposit > 0),
                                    FirstDepositAmount = signUpData.Sum(info => info.FirstDeposit),
                                    TotalDepositAmount = signUpData.Sum(info => info.TotalDeposit)
                                };
                                goal123Data.Children.Add(childData);
                            }
                        }

                        goal123Data.NewlyRegisterUsers = goal123Data.Children.Sum(info => info.NewlyRegisterUsers);
                        goal123Data.DepositUsers = goal123Data.Children.Sum(info => info.DepositUsers);
                        goal123Data.FirstDepositAmount = goal123Data.Children.Sum(info => info.FirstDepositAmount);
                        goal123Data.TotalDepositAmount = goal123Data.Children.Sum(info => info.TotalDepositAmount);
                        nruData.SignUpDataSum.Add(goal123Data);
                    }

                    //APP
                    var goalAppList = landingPagesList.Where(x => x.SignupUrl?.Contains("app") ?? false).ToList();
                    var goalAppData = new SignUpDataSum
                    {
                        SignUpSource = "APP",
                        NewlyRegisterUsers = goalAppList.Count(),
                        DepositUsers = goalAppList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = goalAppList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = goalAppList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(goalAppList).ToList();
                    nruData.SignUpDataSum.Add(goalAppData);

                    //Landing Pages
                    var goalLandingPagesChilds = landingPagesList
                        .GroupBy(i => i.SignupUrl)
                        .Select(i => new SignUpData
                        {
                            SignUpSource = i.First().SignupUrl,
                            NewlyRegisterUsers = i.Count(),
                            DepositUsers = i.Count(info => info.TotalDeposit > 0),
                            FirstDepositAmount = i.Sum(info => info.FirstDeposit),
                            TotalDepositAmount = i.Sum(info => info.TotalDeposit),
                        }).ToList();

                    var goalLandingPagesData = new SignUpDataSum
                    {
                        Children = goalLandingPagesChilds,
                        SignUpSource = "Landing Pages",
                        NewlyRegisterUsers = landingPagesList.Count(),
                        DepositUsers = landingPagesList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = landingPagesList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = landingPagesList.Sum(info => info.TotalDeposit),
                    };
                    nruData.SignUpDataSum.Add(goalLandingPagesData);

                    #endregion

                    break;

                case 3:

                    #region BWING

                    //AFF
                    var bwingAffList = landingPagesList.Where(x => !string.IsNullOrEmpty(x.AffCode)).ToList();
                    var bwingAffData = new SignUpDataSum
                    {
                        SignUpSource = "AFF",
                        NewlyRegisterUsers = bwingAffList.Count,
                        DepositUsers = bwingAffList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = bwingAffList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = bwingAffList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(bwingAffList).ToList();
                    nruData.SignUpDataSum.Add(bwingAffData);

                    //Bong88/Viva88
                    var bwingCreditSite = new List<int>() { 106733 }; //'BwingRef01'
                    var bwingBong88List = landingPagesList.Where(x => bwingCreditSite.Contains(x.AgentId)).ToList();
                    var bwingBong88ListData = new SignUpDataSum
                    {
                        SignUpSource = "Bong88/Viva88",
                        NewlyRegisterUsers = bwingBong88List.Count(),
                        DepositUsers = bwingBong88List.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = bwingBong88List.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = bwingBong88List.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(bwingBong88List).ToList();
                    nruData.SignUpDataSum.Add(bwingBong88ListData);

                    //GroupBy Domain
                    groupUrlDic = _mpaDomainConfig["Bwing"];

                    foreach (var groupUrl in groupUrlDic.Keys)
                    {
                        var bwingData = new SignUpDataSum
                        {
                            Children = new List<SignUpData>(),
                            SignUpSource = groupUrl
                        };

                        var domainList = _mpaDomainConfig["Bwing"][groupUrl];
                        foreach (var domain in domainList)
                        {
                            var signUpData = landingPagesList.Where(x => x.SignupUrl?.Contains(domain) ?? false).ToList();
                            if (signUpData.Count > 0)
                            {
                                landingPagesList = landingPagesList.Except(signUpData).ToList();
                                var childData = new SignUpData
                                {
                                    SignUpSource = domain,
                                    NewlyRegisterUsers = signUpData.Count(),
                                    DepositUsers = signUpData.Count(info => info.TotalDeposit > 0),
                                    FirstDepositAmount = signUpData.Sum(info => info.FirstDeposit),
                                    TotalDepositAmount = signUpData.Sum(info => info.TotalDeposit)
                                };
                                bwingData.Children.Add(childData);
                            }
                        }

                        bwingData.NewlyRegisterUsers = bwingData.Children.Sum(info => info.NewlyRegisterUsers);
                        bwingData.DepositUsers = bwingData.Children.Sum(info => info.DepositUsers);
                        bwingData.FirstDepositAmount = bwingData.Children.Sum(info => info.FirstDepositAmount);
                        bwingData.TotalDepositAmount = bwingData.Children.Sum(info => info.TotalDepositAmount);
                        nruData.SignUpDataSum.Add(bwingData);
                    }

                    //APP
                    var bwingAppList = landingPagesList.Where(x => x.SignupUrl?.Contains("app") ?? false).ToList();
                    var bwingAppData = new SignUpDataSum
                    {
                        SignUpSource = "APP",
                        NewlyRegisterUsers = bwingAppList.Count(),
                        DepositUsers = bwingAppList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = bwingAppList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = bwingAppList.Sum(info => info.TotalDeposit),
                    };
                    landingPagesList = landingPagesList.Except(bwingAppList).ToList();
                    nruData.SignUpDataSum.Add(bwingAppData);

                    //Landing Pages
                    var bwingLandingPagesChilds = landingPagesList
                        .GroupBy(x => x.SignupUrl)
                        .Select(i => new SignUpData
                        {
                            SignUpSource = i.Key,
                            NewlyRegisterUsers = i.Count(),
                            DepositUsers = i.Count(info => info.TotalDeposit > 0),
                            FirstDepositAmount = i.Sum(info => info.FirstDeposit),
                            TotalDepositAmount = i.Sum(info => info.TotalDeposit),
                        }).ToList();

                    var bwingLandingPagesData = new SignUpDataSum
                    {
                        Children = bwingLandingPagesChilds,
                        SignUpSource = "Landing Pages",
                        NewlyRegisterUsers = landingPagesList.Count(),
                        DepositUsers = landingPagesList.Count(info => info.TotalDeposit > 0),
                        FirstDepositAmount = landingPagesList.Sum(info => info.FirstDeposit),
                        TotalDepositAmount = landingPagesList.Sum(info => info.TotalDeposit),
                    };
                    nruData.SignUpDataSum.Add(bwingLandingPagesData);

                    #endregion

                    break;
            }

            if (input.IsCompared)
            {
                var comparedKey = $"MpaMarketingNRU:{input.SiteId}:" +
                                  $"{JsonConvert.SerializeObject(input.CurrencyIds)}:" +
                                  $"{input.ComparedStartDate.DateTime:yyyyMMddHHmmssfff}:" +
                                  $"{input.ComparedEndDate.DateTime:yyyyMMddHHmmssfff}:" +
                                  $"{input.BaseCurrencyId}";

                var comparedData = _redisHelper.TryGetOrCreate(comparedKey, TimeSpan.FromMinutes(15),
                    () => _nruService.GetMpaMarketingNRUInfo(
                        input.SiteId,
                        JsonConvert.SerializeObject(input.CurrencyIds),
                        input.ComparedStartDate.DateTime,
                        input.ComparedEndDate.DateTime,
                        input.BaseCurrencyId));

                nruData.SummaryData.CompareNewlyRegisterUsers = comparedData.Count;
                nruData.SummaryData.ChangeRate = comparedData.Count != 0 ? (nruData.SummaryData.NewlyRegisterUsers / comparedData.Count - 1) * 100 : 0;
            }

            return nruData;
        }

        public List<MpaMarketingNRUBonus> GetMpaMarketingNRUBonus(MpaNRUReportController.GetMarketingNRUDataRequest input)
        {
            var key = $"MpaMarketingNRUBonus:{input.SiteId}:" +
                      $"{JsonConvert.SerializeObject(input.CurrencyIds)}:" +
                      $"{input.StartDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.EndDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.BaseCurrencyId}";

            var data = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => _nruService.GetMpaMarketingNRUBonusInfo(
                    input.SiteId,
                    JsonConvert.SerializeObject(input.CurrencyIds),
                    input.StartDate.DateTime,
                    input.EndDate.DateTime,
                    input.BaseCurrencyId));

            return data;
        }

        public IEnumerable<MpaMarketingNRUBonusDetail> GetMpaMarketingNRUBonusDetail(
            MpaNRUReportController.GetMarketingNRUBonusDetailRequest input,
            out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var masterKey = $"MpaMarketingNRUBonusDetail:{input.GetRedisKey()}";
            var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(15);

            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (input.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<MpaMarketingNRUBonusDetail>(masterKey, 0, -1).OrderBy(input);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, input.OrderBy);
                    }

                    var (sortIndex, sortCount) =
                        pageInfo.ListPagination(sortData.Count(), input.PageNumber, input.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (input.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<MpaMarketingNRUBonusDetail>(masterKey, 0, input.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaMarketingNRUBonusDetail>(masterKey, startIndex, endIndex);
            }

            //Search range table
            var data = _nruService.GetMpaMarketingNRUBonusDetailInfo(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                input.BaseCurrencyId);

            //Sort
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).AsQueryable();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count(), input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }

        #endregion

        #region private function

        private void AddCache(IEnumerable<object> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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

        #endregion

    }
}
