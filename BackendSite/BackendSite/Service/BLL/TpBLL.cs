using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BackendSite.Controllers.Api.TpController;

namespace BackendSite.Service.BLL
{
    public class TpBLL
    {
        private readonly ServerInfoService serverInfoService;
        private readonly CourierAdapter courierAdapter;
        private readonly string apiUrl;
        private readonly ThirdPartyService thirdPartyService;
        private readonly IMemoryCache cache;
        private readonly int _balanceTimeoutSecond;

        public TpBLL(IMemoryCache cache, IConfiguration configuration, CourierAdapter courierAdapter, ThirdPartyService thirdPartyService, ServerInfoService serverInfoService)
        {
            this.courierAdapter = courierAdapter;
            this.apiUrl = configuration.GetValue<string>("TpService");
            this.thirdPartyService = thirdPartyService;
            this.cache = cache;
            this.serverInfoService = serverInfoService;
            _balanceTimeoutSecond = configuration.GetValue<int>("BalanceTimeoutSecond");
        }
        public IEnumerable<TpTransferInfo> GetTpTransferInfoByCust(int CustId, string TransId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return thirdPartyService.GetTpTransferInfoByCust(CustId, TransId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public async Task<IEnumerable<TpWallet>> GetTpWalletAsync(int custId)
        {
            string sKey = $"TpWallet_{custId}";
            var tpWalletList = cache.Get<IEnumerable<TpWallet>>(sKey);
            if (tpWalletList == null)
            {
                tpWalletList = thirdPartyService.GetTpWallet(custId);
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
                    cache.Set(sKey, tpWalletList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));
                }
            }

            return tpWalletList;
        }

        public async Task<ApiResult<TransferResult>> TransferAsync(TpTransfer inTpTransfer)
        {
            decimal amount = inTpTransfer.Amount;
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
            var myUrl = $"{apiUrl}Transfer";
            var data = await courierAdapter.MyPostAsync<TransferResult, TpTransfer>(myUrl, tpTransfer);

            return data;
        }
        public async Task<ApiResult<UserBalance>> GetBalanceAsync(TpInfo inInfo)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            var myUrl = $"{apiUrl}GetBalance";
            var oTpInfo = new TpInfo
            {
                TpId = inInfo.TpId,
                CustId = inInfo.CustId,
                inputs = input
            };

            ApiResult<UserBalance> data = new ApiResult<UserBalance>();

            try
            {
                data = await courierAdapter.MyPostAsync<UserBalance, TpInfo>(myUrl, oTpInfo, _balanceTimeoutSecond);
            }
            catch (Exception ex)
            {
                data.ErrorCode = 9000;
                data.Message = ex.ToString();
            }

            ApiResult<UserBalance> result = data;

            return result;
        }
        public async Task<ApiResult<TransferResult>> FixNegativeAsync(TpInfo inInfo)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            var myUrl = $"{apiUrl}FixNegative";
            var oTpInfo = new TpInfo
            {
                TpId = inInfo.TpId,
                CustId = inInfo.CustId,
                inputs = input
            };

            var result = await courierAdapter.MyPostAsync<TransferResult, TpInfo>(myUrl, oTpInfo);

            return result;
        }
        public IEnumerable<SiteTpListIfo> GetSiteTpList(int SiteId)
        {
            return thirdPartyService.GetSiteTpList(SiteId);
        }
        public async Task<ApiResult<TransferResult>> AdminAdjustmentTransferAsync(AdminTpTransfer inadminTpTransfer)
        {
            decimal amount = inadminTpTransfer.Amount;
            if (amount.ToString().IndexOf(".") > -1)
            {
                decimal.TryParse(amount.ToString("#.##"), out amount);
            }

            var adminTpTransfer = new AdminTpTransfer
            {
                TpId = inadminTpTransfer.TpId,
                CustId = inadminTpTransfer.CustId,
                Amount = amount,
                direction = inadminTpTransfer.direction,
                inputs = null,
                Operator = inadminTpTransfer.Operator,
                Remarks = inadminTpTransfer.Remarks
            };
            var myUrl = $"{apiUrl}AdjustWallet";
            var data = await courierAdapter.MyPostAsync<TransferResult, TpTransfer>(myUrl, adminTpTransfer);

            return data;
        }
        public IEnumerable<AdminTpTransferInfo> GetAdjustmentTpLog(int SiteId, int TpId, int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return thirdPartyService.GetAdjustmentTpLog(SiteId, TpId, CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }

        public IEnumerable<SiteTpListIfo> GetTpBySiteAndCurrency(TpDetailPara TpDetailPara)
        {
            var UserSiteList = serverInfoService.GetSiteList();
            IEnumerable<SiteTpListIfo> result = null;
            if (UserSiteList.Where(x => x.SiteId == TpDetailPara.Site).Select(x => x).ToList().FirstOrDefault().ICurrencyList.Contains(TpDetailPara.Currency))
            {
                result =  thirdPartyService.GetTpBySiteAndCurrency(TpDetailPara);
            }
            return result;
        }
        public void SetAdjustmentTpCustInfoLog(string Menu,int ActionType,string DetailInfo,string Operator)
        {
            thirdPartyService.SetAdjustmentTpCustInfoLog(Menu, ActionType, DetailInfo, Operator);
        }
    }
}
