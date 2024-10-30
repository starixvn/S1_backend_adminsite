using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Store.GiftManagement;
using GetListRequest = BackendSite.Service.Model.Store.GiftManagement.GetListRequest;

namespace BackendSite.Service.BLL
{
    public class GiftManagementBLL
    {
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly string _bucketName;
        private readonly string _folder;
        private readonly string _tpServiceApiUrl;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly ILogger<GiftManagementBLL> _logger;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly GiftMamagementService _giftMamagementService;
        private readonly Dictionary<string, string> _changeGameImgName;
        private readonly BonusCodeBLL _bonusCodeBll;
        private readonly Dictionary<int, string> _siteStoreImageUrl;

        public GiftManagementBLL(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _storeSiteUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").StoreService;
            _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
            _bucketName = configBll.GetConfigValue("AWSBannerBucketName");
            _folder = configBll.GetConfigValue("AWSStoreFolderName");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<GiftManagementBLL>>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _giftMamagementService = provider.GetRequiredService<GiftMamagementService>();
            _bonusCodeBll = provider.GetRequiredService<BonusCodeBLL>();
            _changeGameImgName = new Dictionary<string, string>()
            {
                {"Iconic","AT" },
                {"Kiss918","918Kiss" },
                {"END","ES" },
                {"EBet","WE" },
            };
            _siteStoreImageUrl = configBll.GetConfigValuesBySiteId<string>("StoreImageUrl");
        }

        public Task<ApiResult<object>> GetGiftListAsync(GetListRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetList";
            var result = _courierAdapter.MyPostAsync<object, GetListRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftTagsAsync(GetTagsRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetTags";
            var result = _courierAdapter.MyPostAsync<object, GetTagsRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftCategoriesAsync(GetCategoriesRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetCategories";
            var result = _courierAdapter.MyPostAsync<object, GetCategoriesRequest>(url, req);

            return result;
        }

        public Task<ApiResult<RedemptionLimit>> GetRedemptionLimitAsync(GetRedemptionLimitRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetRedemptionLimit";
            var result = _courierAdapter.MyPostAsync<RedemptionLimit, GetRedemptionLimitRequest>(url, req);

            return result;
        }

        public async Task<ApiResult<IEnumerable<GiftInfo>>> GetGiftAsync(GetGiftRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/Get";
            var result = _courierAdapter.MyPostAsync<IEnumerable<GiftInfo>, GetGiftRequest>(url, req).Result;

            foreach (var item in result.Data)
            {
                var storeImageUrl = _siteStoreImageUrl[item.SiteId];
                item.PictureUrl = $"{storeImageUrl}{item.PictureUrl}";
            }

            return result;
        }

        public async Task<ApiResult> CreateGiftAsync(CreateGiftRequest req, string userName)
        {
            var serverInfo = _serverInfoBll.GetServerInfo(req.SiteId);

            if (req.GiftType == GiftTypeEnum.FreeSpin)
            {
                //創建bonuscode
                var bonusCode = BonusCodeRule(req, serverInfo.SiteName);
                var apiResult = await _bonusCodeBll.CreateBonusCode(req.SiteId,req.CurrencyId,req.TpId,req.GameId,req.Round,bonusCode);
                //失敗return
                if (apiResult.ErrorCode != 0) { return new ApiResult(apiResult.ErrorCode, apiResult.RawMessage); }
                //成功則寫入WorkerPeriod
                _giftMamagementService.SetWorkerPeriod(req);
            }

            var url = _storeSiteUrl + "GiftManagement/Create";
            var data = new CreateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, CreateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        private static string BonusCodeRule(CreateGiftRequest req, string siteName)
        {
            //BonusCode規則。ex.52FREESPIN100FORNOVA8820240306
            var bonusCode = $"{req.TpId}{req.CurrencyId}FREESPIN{req.Round}FOR{siteName.ToUpper()}{DateTime.UtcNow.ToString("yyyyMMdd")}";
            return bonusCode;
        }


        private async Task<ApiResult> CreateBonusCode(CreateGiftRequest req, string bonusCode)
        {
            var currency = await _thirdPartyService.GetTpCurrency(5, req.CurrencyId);
            var inputs = new Dictionary<string, string>();
            var url = _tpServiceApiUrl + "CreateBonusCode";
            inputs.Add("GameId", "vs20olympnova88");//目前只有PP一款遊戲
            inputs.Add("BonusCode", bonusCode);
            inputs.Add("Round", req.Round.ToString());
            inputs.Add("Currency", currency.TpCurrencyId);
            inputs.Add("ExpirationDay", "8");//freespin有效期限8天
            var request = new CreateBonusCodeParams()
            {
                SiteId = req.SiteId,
                TpId = 5,//PP
                inputs = inputs
            };

            var apiResult = await _courierAdapter.MyPostAsync<object, CreateBonusCodeParams>(url, request);
            _logger.LogInformation(apiResult.ErrorCode == 0
                ? $"BonusCode:{bonusCode} Create Success"
                : $"BonusCode:{bonusCode} Create Fail");

            return apiResult;
        }


        public async Task<ApiResult> UpdateGiftAsync(UpdateGiftRequest req, string userName)
        {
            var url = _storeSiteUrl + "GiftManagement/Update";
            var storeImageUrl = _siteStoreImageUrl[req.SiteId];
            foreach (var item in req.Infos)
            {
                int count = (item.PictureUrl.Length- item.PictureUrl.Replace(storeImageUrl, "").Length) / storeImageUrl.Length;
                item.PictureUrl = item.PictureUrl.Substring(count * storeImageUrl.Length);
            }
            var data = new UpdateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, UpdateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateCategoryAsync(CreateCategoryRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/CreateCategory";
            var result = await _courierAdapter.MyPostAsync<object, CreateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateCategoryAsync(UpdateCategoryRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/UpdateCategory";
            var result = await _courierAdapter.MyPostAsync<object, UpdateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateTagAsync(CreateTagRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/CreateTag";
            var result = await _courierAdapter.MyPostAsync<object, CreateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateTagAsync(UpdateTagRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/UpdateTag";
            var result = await _courierAdapter.MyPostAsync<object, UpdateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> SetRedemptionLimitAsync(SetRedemptionLimitRequest req, string userName)
        {
            var url = _storeSiteUrl + "GiftManagement/SetRedemptionLimit";
            var data = new SetRedemptionLimitParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, SetRedemptionLimitParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<string> UploadPictureAsync(IFormFile file)
        {
            if (!CheckIsImage(file))
            {
                return string.Empty;
            }

            var ext = Path.GetExtension(file.FileName);
            var path = $"/store/gift/{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            var key = _folder + path;
            var success = await _awsS3Helper.UploadFileAsync(file, _bucketName, key);
            var result = success ? path : string.Empty;

            return result;
        }

        public async Task<ApiResult> SetGiftSortAsync(SetSortRequest req, string userName)
        {
            var index = 1;
            var sortInfos = req.SortedIdList.Select(x => new SortItem
            {
                Sort = index++,
                GiftId = x
            });

            var param = new SetSortParams(sortInfos, userName);
            var url = _storeSiteUrl + "GiftManagement/SetSort";
            var result = await _courierAdapter.MyPostAsync<object, SetSortParams>(url, param);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        private bool CheckIsImage(IFormFile file)
            => file.ContentType.StartsWith("image/");

        public IEnumerable<ProvideGameList> GetGameName(CreateGiftRequest req)
        {
            return _giftMamagementService.GetGameName(req.SiteId,req.CurrencyId)
                .GroupBy(p => p.TpId)
                .Select(g => new ProvideGameList
                {
                    TpId = g.Key,
                    TpName = _changeGameImgName.TryGetValue(g.First().TpName, out var changeName) ? changeName : g.First().TpName,
                    data = g.Select(p => new ProvideGameListDto
                    {
                        SlotId = p.SlotId,
                        TpId = p.TpId,
                        TpName = _changeGameImgName.TryGetValue(p.TpName,out var changeName) ? changeName : p.TpName,
                        GameName = p.GameName,
                        GameId = p.GameId,
                    })
                }
                );
        }
    }
}