using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa.Extension;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaWinLossBLL : IMpaWinLossBLL
    {
        private readonly ICacheHelper _redisHelper;
        private readonly ILogger<MpaWinLossBLL> _logger;
        private readonly CourierAdapter _courierAdapter;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly string _reportApiUrl;
        private readonly Common _common;
        private readonly CustInfoBLL _custInfoBll;

        public MpaWinLossBLL(IServiceProvider provider)
        {
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _logger = provider.GetRequiredService<ILogger<MpaWinLossBLL>>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _common = provider.GetRequiredService<Common>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _reportApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").ReportService;
        }

        #region Get Report Result

        public async Task<IEnumerable<MpaWinLossResponseForFrontEnd>> GetMapWinLossResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);

            return data.SortThen2ApiFormat(request);
        }

        public async Task<IEnumerable<MpaWinLossResponseForFrontEndSimple>> GetMapWinLossResultSimple(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);

            return data.SortThen2ApiFormatSimple(request);
        }

        public async Task<MpaWinLossResponseToFrontEndWithPlayerWrapper> GetMpaWinLossResultWithPlayerInfo(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            IPageInfo pageInfo = new PageInfo();
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var apiData = data.SortThen2ApiFormatWithPlayer(request).ToList();

            if (request.PageNumber != -1)
            {
                var (index, count) = pageInfo.ListPagination(apiData.Count, request.PageNumber, request.PageSize);
                apiData = apiData.GetRange(index, count);
            }

            return new MpaWinLossResponseToFrontEndWithPlayerWrapper
            {
                Data = apiData,
                RowCount = data.Result.Count()
            };
        }

        public async Task<IEnumerable<MpaWinLossResponseForFrontEndWithPlayerDetail>> GetWinLossByPlayerDetail(
            MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
            request.UserName = custInfo.UserName;
            request.SiteIds = new[] { custInfo.SiteId };

            var data = await GetWinLossReportAsync(request, groupByType);

            return data.Result.Select(x => new MpaWinLossResponseForFrontEndWithPlayerDetail(x));
        }

        #endregion

        #region Export Result

        public async Task<IEnumerable<MpaWinLossByDateDtoExport>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByDateDtoExport>();
                r.Date = x.Key;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByDateDownloadDtoExport>> GetMpaWinLossByDateDownload(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByDateDownloadDtoExport>();
                r.Date = x.Key;
                r.Currency = x.ChildKey;
                r.Brand = x.Brand;
                r.TpCategory = x.TpCategory;
                r.TpGameType = x.TpGameType;
                r.UserName = x.UserName;
                r.TpUserName = x.TpUserName;
                return r;
            });

            return result;
        }
        public async Task<IEnumerable<MpaWinLossByTpGameTypeDtoExport>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormatSimple<MpaWinLossByTpGameTypeDtoExport>();
                r.TpGameTypeName = x.Key;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByTpCategoryDtoExport>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormatSimple<MpaWinLossByTpCategoryDtoExport>();
                r.TpCategoryName = x.Key;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByPlayerDtoExport>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByPlayerDtoExport>();
                r.UserName = x.UserName;
                r.TpUserName = x.TpUserName;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByCurrencyDtoExport>> GetMapWinLossByCurrencyExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByCurrencyDtoExport>();
                r.Currency = x.Key;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByBrandDtoExport>> GetMpaWinLossByBrandExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            GetOrderByProp(request);

            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByBrandDtoExport>();
                r.SiteName = x.Key;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        public async Task<IEnumerable<MpaWinLossByAffiliateDtoExport>> GetMpaWinLossByAffiliateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            GetOrderByProp(request);
            var data = await GetWinLossReportAsync(request, groupByType);
            var sortedData = data.SortThen2ExcelData(request);
            var result = sortedData.Select(x =>
            {
                var r = x.ToExcelFormat<MpaWinLossByAffiliateDtoExport>();
                r.AffiliateAccount = x.Key;
                r.UserName = x.UserName;
                r.TpUserName = x.TpUserName;
                r.Currency = x.ChildKey;

                return r;
            });

            return result;
        }

        #endregion


        #region ReportService

        private async Task<MpaWinLossReportResponse> GetWinLossReportAsync(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var result = await _courierAdapter
                .MyPostAsync<MpaWinLossReportResponse, MpaWinLossReportRequest>($"{_reportApiUrl}MpaWinLossReport/Get",
                    new MpaWinLossReportRequest
                    {
                        SiteIds = request.SiteIds,
                        CurrencyIds = request.CurrencyIds,
                        AffiliateNames = string.IsNullOrEmpty(request.AffiliateAccount)
                            ? ArraySegment<string>.Empty : new[] { request.AffiliateAccount },
                        UserNames = string.IsNullOrEmpty(request.UserName)
                            ? ArraySegment<string>.Empty : new[] { request.UserName },
                        DateType = request.DateType,
                        StartTime = request.StarTime,
                        EndTime = request.EndTime,
                        IncludeTest = request.IncludeTest,
                        MemberTypes = request.MemberTypes,
                        NoDraw = request.NoDraw,
                        TpCategorys = request.TpCategorys,
                        BaseCurrencyId = request.BaseCurrencyId,
                        GroupByType = groupByType,
                        IncludeFreeSpins = request.IncludeFreeSpins,
                        TpGameTypes = request.TpGameTypes,
                        DisplayGroupBreakdownInOriginalCurrency = request.DisplayGroupBreakdownInOriginalCurrency
                    }, timeout: 1200);

            if (result.ErrorCode != (int)ErrorCodeEnum.Success)
            {
                throw new AdminException($"Get WinLossReport Failed. ({result.ErrorCode}:{result.RawMessage})");
            }

            return result.Data;
        }

        #endregion

        private static void GetOrderByProp(IOrderBy orderByOpt)
        {
            if (!orderByOpt.OrderBy || string.IsNullOrEmpty(orderByOpt.OrderByProperty))
            {
                orderByOpt.OrderBy = true;
                orderByOpt.OrderByProperty = "key";

                return;
            }

            orderByOpt.OrderByProperty = orderByOpt.OrderByProperty.Equals("sabaId", StringComparison.OrdinalIgnoreCase)
                ? "tpUserName" : orderByOpt.OrderByProperty;
        }
    }
}
