using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.TpMaintain;

using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Service.BLL
{
    public class TpBLL
    {
        private readonly ServerInfoService _serverInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _apiUrl;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly ICacheHelper _redisHelper;
        private readonly int _balanceTimeoutSecond;
        private readonly ILogger<TpBLL> _logger;

        public TpBLL(IServiceProvider provider)
        {
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _apiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("TpService");
            _balanceTimeoutSecond = provider.GetRequiredService<IConfiguration>().GetValue<int>("BalanceTimeoutSecond");
            _logger = provider.GetRequiredService<ILogger<TpBLL>>();
        }

        public IEnumerable<TpTransferInfo> GetTpTransferInfoByCust(int CustId,
            string TransId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _thirdPartyService.GetTpTransferInfoByCust(CustId,
                TransId,
                startDate,
                endDate,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public async Task<IEnumerable<TpWallet>> GetTransferTpWalletWithCacheAsync(int custId)
        {
            var key = $"TransferTpWallet_{custId}";

            return await _redisHelper.TryGetOrCreateAsync(key, TimeSpan.FromSeconds(10),
                () => GetTransferTpWalletAsync(custId));
        }

        private async Task<IEnumerable<TpWallet>> GetTransferTpWalletAsync(int custId)
        {
            var tpWalletList = await _thirdPartyService.GetTransferTpWallet(custId);

            var tasks = tpWalletList
                .Where(x => !string.IsNullOrEmpty(x.TpUserName))
                .Select(async x =>
                {
                    var tpInfo = new TpInfo
                    {
                        TpId = x.TpId,
                        CustId = custId,
                    };

                    var apiResult = await GetBalanceAsync(tpInfo);

                    if (apiResult.Data != null)
                    {
                        x.Balance = apiResult.Data.balance;
                    }
                });

            await Task.WhenAll(tasks);

            return tpWalletList;
        }

        public async Task<IEnumerable<TpWallet>> TransferAllTpWalletAsync(int custId)
        {
            var tpWalletList = await _thirdPartyService.GetTransferTpWallet(custId);

            try
            {
                var tasks = tpWalletList
                    .Where(x => !string.IsNullOrEmpty(x.TpUserName))
                    .Select(async x =>
                    {
                        var tpInfo = new TpInfo
                        {
                            TpId = x.TpId,
                            CustId = custId
                        };

                        var apiResult = await GetBalanceAsync(tpInfo);

                        if (apiResult?.Data is { balance: > 0 })
                        {
                            var tpTransfer = new TpTransfer
                            {
                                TpId = x.TpId,
                                CustId = custId,
                                Amount = apiResult.Data.balance,
                                direction = TpDirection.ToMain,
                                inputs = null
                            };
                            var result = await TransferAsync(tpTransfer);

                            if (result.Data != null)
                            {
                                x.Balance = 0;
                            }
                        }
                    });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TransferAllTpWallet failed ");
            }

            return tpWalletList;
        }

        public async Task<ApiResult<TransferResult>> TransferAsync(TpTransfer inTpTransfer)
        {
            var amount = inTpTransfer.Amount;
            if (amount.ToString().IndexOf(".") > -1)
            {
                decimal.TryParse(amount.ToString("#.##"), out amount);
            }

            var tpTransfer = new TpTransfer
            {
                TpId = inTpTransfer.TpId,
                CustId = inTpTransfer.CustId,
                Amount = amount,
                direction = inTpTransfer.direction,
                inputs = null
            };
            var myUrl = $"{_apiUrl}Transfer";
            var data = await _courierAdapter.MyPostAsync<TransferResult, TpTransfer>(myUrl, tpTransfer);

            return data;
        }
        public async Task<ApiResult<UserBalance>> GetBalanceAsync(TpInfo inInfo)
        {
            var input = new Dictionary<string, string>();
            var myUrl = $"{_apiUrl}GetBalance";

            var oTpInfo = new TpInfo
            {
                TpId = inInfo.TpId,
                CustId = inInfo.CustId,
                inputs = input
            };

            var data = new ApiResult<UserBalance>();

            try
            {
                data = await _courierAdapter.MyPostAsync<UserBalance, TpInfo>(myUrl, oTpInfo, _balanceTimeoutSecond);
            }
            catch (Exception ex)
            {
                data.ErrorCode = 9000;
                data.Message = ex.ToString();
            }

            var result = data;

            return result;
        }
        public async Task<ApiResult<TransferResult>> FixNegativeAsync(TpInfo inInfo)
        {
            var input = new Dictionary<string, string>();
            var myUrl = $"{_apiUrl}FixNegative";
            var oTpInfo = new TpInfo
            {
                TpId = inInfo.TpId,
                CustId = inInfo.CustId,
                inputs = input
            };

            var result = await _courierAdapter.MyPostAsync<TransferResult, TpInfo>(myUrl, oTpInfo);

            return result;
        }
        public IEnumerable<SiteTpListIfo> GetSiteTpList(int siteId)
        {
            return _thirdPartyService.GetSiteTpList(siteId);
        }

        public async Task<ApiResult<AdjustWalletResult>> AdminAdjustmentTransferAsync(AdminTpTransfer inAdminTpTransfer)
        {
            var amount = inAdminTpTransfer.Amount;
            if (amount.ToString().IndexOf(".") > -1)
            {
                decimal.TryParse(amount.ToString("#.##"), out amount);
            }

            var adminTpTransfer = new AdminTpTransfer
            {
                TpId = inAdminTpTransfer.TpId,
                CustId = inAdminTpTransfer.CustId,
                Amount = amount,
                direction = inAdminTpTransfer.direction,
                inputs = null,
                Operator = inAdminTpTransfer.Operator,
                Remarks = inAdminTpTransfer.Remarks,
                Category = inAdminTpTransfer.Category,
                DepositId = inAdminTpTransfer.DepositId,
                IsDepositId = inAdminTpTransfer.IsDepositId,
            };
            var myUrl = $"{_apiUrl}AdjustWallet";
            var data = await _courierAdapter.MyPostAsync<AdjustWalletResult, TpTransfer>(myUrl, adminTpTransfer);

            return data;
        }
        public IEnumerable<AdminTpTransferInfo> GetAdjustmentTpLog(int siteId, int tpId, int custId, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            return _thirdPartyService.GetAdjustmentTpLog(siteId, tpId, custId, pageSize, ref pageNumber, ref totalPages, ref rowCount);
        }

        public IEnumerable<TpAdjustmentLog> GetCustAdjustmentTpLog(GetCustTpAdjustmentLogParams input,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var absPageSize = Math.Abs(pageSize);
            var startIndex = (Math.Abs(pageNumber) - 1) * absPageSize;
            return _thirdPartyService.GetCustTpAdjustmentLog(input, absPageSize, startIndex, out totalPages, out rowCount);
        }

        public IEnumerable<SiteTpListIfo> GetTpBySiteAndCurrency(int siteId, int currency)
        {
            var UserSiteList = _serverInfoService.GetSiteList();
            IEnumerable<SiteTpListIfo> result = null;
            if (UserSiteList.Where(x => x.SiteId == siteId).Select(x => x).ToList().FirstOrDefault().ICurrencyList.Contains(currency))
            {
                result = _thirdPartyService.GetTpBySiteAndCurrency(siteId, currency);
            }
            return result;
        }
        public void SetAdjustmentTpCustInfoLog(string menu, int actionType, string detailInfo, string @operator)
        {
            _thirdPartyService.SetAdjustmentTpCustInfoLog(menu, actionType, detailInfo, @operator);
        }

        public async Task<ApiResult<dynamic>> GetBetDetail(GetTpBetRecordDetailRequest request)
        {
            var myUrl = $"{_apiUrl}GetBetDetail";

            try
            {
                return await _courierAdapter.MyPostAsync<dynamic, GetTpBetRecordDetailRequest>(myUrl, request);
            }
            catch (Exception ex)
            {
                return new ApiResult<dynamic>()
                {
                    ErrorCode = 9000,
                    Message = "AdminSite Error: " + ex.Message
                };
            }
        }

        public async Task<IEnumerable<TpCustInfo>> GetTpCustInfoList(int custId)
        {
            return await _thirdPartyService.GetTpCustInfoListByCustId(custId);
        }

        public Task<IEnumerable<TpCategory>> GetTpCategoryListAsync(int siteId)
        {
            var key = $"TpCategory_{siteId}";

            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(5),
                () => GetSiteTpCategoryListAsync(siteId));
        }

        private async Task<IEnumerable<TpCategory>> GetSiteTpCategoryListAsync(int siteId)
        {
            var tpList = await _thirdPartyService.GetSiteTpListAllAsync(siteId);
            var tpGameTypeList = await _thirdPartyService.GetTpGameType();
            var result = tpGameTypeList
                .Where(x => tpList.Any(y => y.TpId == x.TpId))
                .Select(x => new TpCategory
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName
                })
                .Distinct(new TpCategoryComparer())
                .OrderBy(x => x.CategoryId);

            return result;
        }

        public Task<IEnumerable<TpGameType>> GetTpGameTypeListByCategoryAsync(int categoryId)
        {
            return _thirdPartyService.GetTpGameTypeListByCategory(categoryId);
        }

        public async Task<IEnumerable<TpGameTypeCurrency>> GetTpGameTypeCurremcyListAsync(int siteId, int currencyId)
        {
            var result = await _thirdPartyService.GetTpGameTypeCurrency();
            return result.Where(x => x.SiteId == siteId && x.CurrencyId == currencyId);
        }
        public async Task<IEnumerable<GameTypeBetLimit>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var tpGameType = await _thirdPartyService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var gameTypeWithBetLimit = tpGameType
                .Select(a => new GameTypeBetLimit()
                {
                    TypeId = a.Type,
                    TypeName = a.TypeName
                });
            return gameTypeWithBetLimit;
        }
        public async Task<IEnumerable<SiteTpListIfo>> GetTpListByCategoryAsync(int categoryId)
        {
            var tpGameType = await _thirdPartyService.GetTpListByCategoryAsync(categoryId);

            return tpGameType;
        }
    }
}
