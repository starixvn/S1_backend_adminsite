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
using Newtonsoft.Json;
using static BackendSite.Controllers.Api.TpController;
using BackendSite.Service.Model.enums;

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

        public TpBLL(IServiceProvider provider)
        {
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _cache = provider.GetRequiredService<IMemoryCache>();
            _apiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("TpService");
            _balanceTimeoutSecond = provider.GetRequiredService<IConfiguration>().GetValue<int>("BalanceTimeoutSecond");
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
                foreach (var tpWallet in tpWalletList)
                {
                    try
                    {
                        var tpInfo = new TpInfo
                        {
                            TpId = tpWallet.TpId,
                            CustId = custId,
                        };
                        var apiResult = await GetBalanceAsync(tpInfo);
                        if (apiResult.ErrorCode == 0)
                        {
                            tpWallet.Balance = apiResult.Data.balance;
                        }

                    }
                    catch (Exception)
                    {

                    }
                }

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
                Remarks = inAdminTpTransfer.Remarks
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

        public async Task<ApiResult> SetBetLimitAsync(int tpId, int custId, int minBet, int maxBet)
        {
            var sportType = new List<string>()
            {
                "1", "2", "3", "5", "8", "10", "11", "43", "99", "99MP", "1MP"
            };
            var betSetting = sportType.Select(type => new SabaBetSetting()
            {
                sport_type = type,
                min_bet = minBet,
                max_bet = maxBet,
                max_bet_per_match = maxBet
            }).ToList();
            var myUrl = $"{_apiUrl}SetBetLimit";
            var setBetLimit = new SetBetLimit()
            {
                TpId = tpId,
                CustId = custId,
                BetSetting = JsonConvert.SerializeObject(betSetting)
            };

            try
            {
                var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>(myUrl, setBetLimit);
                return apiResult;
            }
            catch (Exception ex)
            {
                return new ApiResult()
                {
                    ErrorCode = 9000,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResult> SetBetLimitAsync(int tpId, int custId, List<SabaBetSettingResponse> betSettings)
        {
            var sportType = new List<string>()
            {
                "1", "2", "3", "5", "8", "10", "11", "43", "99", "99MP", "1MP"
            };

            var tempSetting = betSettings
                .Where(a => sportType.Contains(a.sport_type))
                .Select(b => new SabaBetSetting()
                {
                    sport_type = b.sport_type,
                    min_bet = decimal.ToInt32(b.min_bet),
                    max_bet = decimal.ToInt32(b.max_bet),
                    max_bet_per_match = decimal.ToInt32(b.max_bet_per_match)
                });

            var myUrl = $"{_apiUrl}SetBetLimit";
            var setBetLimit = new SetBetLimit()
            {
                TpId = tpId,
                CustId = custId,
                BetSetting = JsonConvert.SerializeObject(tempSetting)
            };

            try
            {
                var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>(myUrl, setBetLimit);
                return apiResult;
            }
            catch (Exception ex)
            {
                return new ApiResult()
                {
                    ErrorCode = 9000,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResult<List<SabaBetSettingResponse>>> GetBetLimitAsync(int tpId, int custId)
        {
            var myUrl = $"{_apiUrl}GetBetLimit";
            var setBetLimit = new GetBetLimitReq()
            {
                TpId = tpId,
                CustId = custId
            };

            try
            {
                var apiResult =
                    await _courierAdapter.MyPostAsync<List<SabaBetSettingResponse>, GetBetLimitReq>(myUrl, setBetLimit);
                return apiResult;
            }
            catch (Exception ex)
            {
                return new ApiResult<List<SabaBetSettingResponse>>()
                {
                    ErrorCode = 9000,
                    Message = ex.Message
                };
            }
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
                tpWalletList = _thirdPartyService.GetTpWallet(custId).Where(x=>x.WalletType == (int)WalletTypeEnum.Transfer);
                foreach (var tpWallet in tpWalletList)
                {
                    try
                    {
                        var tpInfo = new TpInfo
                        {
                            TpId = tpWallet.TpId,
                            CustId = custId,
                        };
                        var apiResult = await GetBalanceAsync(tpInfo);
                        if (apiResult.ErrorCode == 0)
                        {
                            tpWallet.Balance = apiResult.Data.balance;
                        }

                    }
                    catch (Exception)
                    {

                    }
                }

                if (tpWalletList != null)
                {
                    _cache.Set(sKey, tpWalletList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));
                }
            }
            return tpWalletList;
        }
    }
}
