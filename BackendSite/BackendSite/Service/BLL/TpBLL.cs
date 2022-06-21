using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static BackendSite.Controllers.Api.TpController;
using BackendSite.Service.Model.enums;
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
        private readonly IMemoryCache _cache;
        private readonly int _balanceTimeoutSecond;
        private CacheHelper _cacheHelper;

        public TpBLL(IServiceProvider provider)
        {
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _cache = provider.GetRequiredService<IMemoryCache>();
            _apiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("TpService");
            _balanceTimeoutSecond = provider.GetRequiredService<IConfiguration>().GetValue<int>("BalanceTimeoutSecond");
            _cacheHelper = provider.GetRequiredService<CacheHelper>();
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
        public async Task<IEnumerable<TpWallet>> GetTpWalletAsync(int custId)
        {
            var sKey = $"TpWallet_{custId}";
            var tpWalletList = _cache.Get<IEnumerable<TpWallet>>(sKey);
            if (tpWalletList == null)
            {
                tpWalletList = _thirdPartyService.GetTpWallet(custId);
                var tasks = tpWalletList.Select(async x =>
                {
                    if(x.WalletType == (int)TpListWalletTypeEnum.FundTransfer) 
                    {
                        var tpInfo = new TpInfo
                        {
                            TpId = x.TpId,
                            CustId = custId,
                        };
                        var apiResult = await GetBalanceAsync(tpInfo);
                        if(apiResult.Data != null)
                        {
                            x.Balance = apiResult.Data.balance;
                        }                    
                    }
                });
                await Task.WhenAll(tasks);

                if (tpWalletList != null)
                {
                    _cache.Set(sKey, tpWalletList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));
                }
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
        
        public async Task<ApiResult<TransferResult>> AdminAdjustmentTransferAsync(AdminTpTransfer inAdminTpTransfer)
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
                Category = inAdminTpTransfer.Category
            };
            var myUrl = $"{_apiUrl}AdjustWallet";
            var data = await _courierAdapter.MyPostAsync<TransferResult, TpTransfer>(myUrl, adminTpTransfer);

            return data;
        }
        public IEnumerable<AdminTpTransferInfo> GetAdjustmentTpLog(int siteId, int tpId, int custId, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            return _thirdPartyService.GetAdjustmentTpLog(siteId, tpId, custId, pageSize, ref pageNumber, ref totalPages, ref rowCount);
        }

        public IEnumerable<SiteTpListIfo> GetTpBySiteAndCurrency(TpDetailPara TpDetailPara)
        {
            var UserSiteList = _serverInfoService.GetSiteList();
            IEnumerable<SiteTpListIfo> result = null;
            if (UserSiteList.Where(x => x.SiteId == TpDetailPara.Site).Select(x => x).ToList().FirstOrDefault().ICurrencyList.Contains(TpDetailPara.Currency))
            {
                result = _thirdPartyService.GetTpBySiteAndCurrency(TpDetailPara);
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
        
        public async Task<IEnumerable<TpWallet>> GetTransferWalletAsync(int custId)
        {
            var sKey = $"TpWallet_{custId}";
            var tpWalletList = _cache.Get<IEnumerable<TpWallet>>(sKey);
            if (tpWalletList == null)
            {
                tpWalletList = _thirdPartyService.GetTpWallet(custId).Where(x => x.WalletType == (int)TpListWalletTypeEnum.FundTransfer);
                var tasks = tpWalletList.Select(async x =>
                {
                    var tpInfo = new TpInfo
                    {
                        TpId = x.TpId,
                        CustId = custId,
                    };
                    var apiResult = await GetBalanceAsync(tpInfo);
                    if (apiResult.ErrorCode == 0)
                    {
                        x.Balance = apiResult.Data.balance;
                    }
                });
                await Task.WhenAll(tasks);

                if (tpWalletList != null)
                {
                    _cache.Set(sKey, tpWalletList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));
                }
            }
            return tpWalletList;
        }
        public Task<IEnumerable<TpCategory>> GetTpCategoryListAsync(int siteId)
        {
            var key = $"TpCategory_{siteId}";

            return _cacheHelper.TryGetOrCreate(key,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)),
                i => GetSiteTpCategoryListAsync(siteId));
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
    }
}
