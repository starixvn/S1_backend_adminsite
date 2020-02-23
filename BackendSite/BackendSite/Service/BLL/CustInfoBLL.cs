using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class CustInfoBLL
    {
        private readonly CustInfoService custInfoService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly IMemoryCache cache;
        private readonly TpBLL tpBLL;
        private readonly WalletService walletService;
        private readonly Tools tools;
        private readonly CodeGenerator codeGenerator;
        private readonly PromotionService promotionService;
        private readonly RollOverService rolloverService;

        public CustInfoBLL(IMemoryCache cache, WalletService walletService, CustInfoService custInfoService, ServerInfoBLL serverInfoBLL, TpBLL tpBLL, Tools tools, CodeGenerator codeGenerator, PromotionService promotionService,RollOverService rolloverService)
        {
            this.custInfoService = custInfoService;
            this.serverInfoBLL = serverInfoBLL;
            this.cache = cache;
            this.tpBLL = tpBLL;
            this.walletService = walletService;
            this.tools = tools;
            this.codeGenerator = codeGenerator;
            this.promotionService = promotionService;
            this.rolloverService = rolloverService;
        }

        public Customer GetCustInfoWithCache(int custId)
        {
            string key = $"CustInfo_{custId}";
            var customer = cache.Get<Customer>(key);
            if (customer == null)
            {
                customer = custInfoService.GetCustInfo(custId);
                if (customer != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));
                    cache.Set(key, customer, cacheEntryOptions);
                }
            }

            return customer;
        }
        public int GetVipLevelWithCache(int custId)
        {
            string key = $"VipLevelCache_{custId}";
            var vipLevelCache = cache.Get<int?>(key) ?? -1;
            if (vipLevelCache != -1)
            {
                return vipLevelCache;
            }

            vipLevelCache = custInfoService.GetVipLevel(custId);
            if (vipLevelCache != 0)
            {
                cache.Set(key, vipLevelCache, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
            }

            return vipLevelCache;
        }
        public IEnumerable<CustInfoDetail> GetCustInfoList(string userName, string transId, string TpUserName, string Email, string PhoneNo, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return custInfoService.GetCustInfoList(userName, transId, TpUserName, Email, PhoneNo, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(string userName, string transId, string TpUserName, string Email, string PhoneNo)
        {
            return custInfoService.GetCustInfoExportList(userName, transId, TpUserName, Email, PhoneNo);
        }

        public async Task<CustInfoDetail> GetCustInfoDetailAsync(int custId, string userName)
        {
            var custInfoDetail = custInfoService.GetCustInfoDetail(custId, userName);
            if (custInfoDetail != null)
            {
                custInfoDetail.tpWallet = await tpBLL.GetTpWalletAsync(custInfoDetail.CustId);
                custInfoDetail.promotions = promotionService.GetPromotionListByCustId(custInfoDetail.CustId);
                if (custInfoDetail.promotions != null)
                {
                    foreach(var promotion in custInfoDetail.promotions)
                    {
                        promotion.custRollOver = rolloverService.GetCustRollOver(custInfoDetail.CustId, promotion.BonusCode);
                    }
                }
            }
            return custInfoDetail;
        }

        public decimal GetBalance(int custId)
        {
            return walletService.GetBalance(custId);
        }
        public int SetCustStatusSwitch(int custId, int Op)
        {
            return custInfoService.SetCustStatusSwitch(custId, Op);
        }

        public CustPhotoId GetCustPhotoId(int custId)
        {
            return custInfoService.GetCustPhotoId(custId);
        }
        public bool DelCustPhotoId(int custId)
        {
            return custInfoService.DelCustPhotoId(custId);
        }
        public bool AddCustNote(int custId, string Note, string Operator)
        {
            return custInfoService.AddCustNote(custId, Note, Operator);
        }
        public IEnumerable<CustNote> GetCustNotes(int custId)
        {
            return custInfoService.GetCustNotes(custId);
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return custInfoService.GetLoginLogByCust(CustId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public bool SetResetPwd(int CustId, out string NewPassword)
        {
            codeGenerator.Maximum = 8;
            codeGenerator.Minimum = 6;
            codeGenerator.RepeatCharacters = true;
            NewPassword = codeGenerator.Generate();
            string md5NewPassword = tools.MD5Encrypt(NewPassword);
            return custInfoService.SetResetPwd(CustId, md5NewPassword);
        }
        public bool ResetProfile(int CustId)
        {
            return custInfoService.ResetProfile(CustId);
        }
        public bool ResetCustBankInfo(int CustId)
        {
            return custInfoService.ResetCustBankInfo(CustId);
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            return custInfoService.GetWithdrawalAvailableBalance(custId);
        }
        public int CancelBonusByBonusId(int bonusId)
        {
            return promotionService.CancelBonusByBonusId(bonusId);
        }
    }
}
