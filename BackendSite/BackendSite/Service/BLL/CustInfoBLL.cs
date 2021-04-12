using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
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
        private readonly CustTurnoverService custTurnoverService;
        public CustInfoBLL(IMemoryCache cache, WalletService walletService, CustInfoService custInfoService, ServerInfoBLL serverInfoBLL, TpBLL tpBLL, Tools tools, CodeGenerator codeGenerator, PromotionService promotionService, RollOverService rolloverService, CustTurnoverService custTurnoverService)
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
            this.custTurnoverService = custTurnoverService;
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
        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, int pageSize, int? affId, IEnumerable<int> selectCurrency, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            List<int> currncyList = new List<int>();
            serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x =>
            {
                currncyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            return custInfoService.GetCustInfoList(selectSite, birDate, birMonth, birYear, userName, AccountName, LoginIp, transId, TpUserName, Email, PhoneNo, startDate, endDate, pageSize, affId, selectCurrency,ref pageNumber, out totalPages, out rowCount, JsonConvert.SerializeObject(siteList), JsonConvert.SerializeObject(currncyList.Select(x => x).Distinct().ToList()));
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, int? AffId)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });

            return custInfoService.GetCustInfoExportList(selectSite, birDate, birMonth, birYear, userName, AccountName, LoginIp, transId, TpUserName, Email, PhoneNo, startDate, endDate, JsonConvert.SerializeObject(siteList), AffId);
        }

        public async Task<CustInfoDetail> GetCustInfoDetailAsync(int custId, string userName)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.tpWallet = await tpBLL.GetTpWalletAsync(custInfoDetail.CustId);
                custInfoDetail.promotions = promotionService.GetPromotionListByCustId(custInfoDetail.CustId);
                if (custInfoDetail.promotions != null)
                {
                    foreach (var promotion in custInfoDetail.promotions)
                    {
                        if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                        {
                            promotion.custRollOver = rolloverService.GetCustRollOver(custInfoDetail.CustId, promotion.BonusCode);
                            if (promotion.BonusInfoStatus == 5)
                            {
                                promotion.BonusInfoStatus = 1;
                            }
                        }
                        else
                        {
                             promotion.custRollOver = new CustRollOver() { RollOver = 0,TurnOver = 0 };
                        };
                        
                    }
                }
            }
            return custInfoDetail;
        }

        public CustInfoDetail GetCustInfoDetails(int? custId, string userName, int? siteId)
        {
            return custInfoService.GetCustInfos(custId, userName, siteId);
        }

        public decimal GetBalance(int custId)
        {
            return walletService.GetBalance(custId);
        }
        public int SetCustStatusSwitch(int custId, int Op)
        {
            return custInfoService.SetCustStatusSwitch(custId, Op);
        }
        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, string Operator)
        {
            return custInfoService.SetCustInfo(custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, Operator);
        }
        public CustPhotoId GetCustPhotoId(int custId)
        {
            return custInfoService.GetCustPhotoId(custId);
        }
        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            return custInfoService.SetCustPhotoId(custId, siteId, picFileName, photoIdPic);
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
        public bool SetCustNotes(int custId, int noteId, string note, int type, string Operator)
        {
            return custInfoService.SetCustNotes(custId, noteId, note, type, Operator);
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
        public bool SetCustStatus(int CustId, int status)
        {
            return custInfoService.SetCustStatus(CustId, status);
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            return custInfoService.GetWithdrawalAvailableBalance(custId);
        }
        public int CancelBonusByBonusId(int bonusId)
        {
            return promotionService.CancelBonusByBonusId(bonusId);
        }

        public List<CustTurnover> GetCustTurnover(int CustId, string[] GameTypes, int StartTime, int EndTime)
        {
            var datas = custTurnoverService.GetCustTurnover(CustId, GameTypes, StartTime, EndTime);

            var result = new List<CustTurnover>();

            foreach (CustTurnover data in datas)
            {
                var item = result.FirstOrDefault(x => x.GameType == data.GameType);
                if (item == null)
                {
                    result.Add(data);
                }
                else
                {
                    item.Stake += data.Stake;
                    item.WinlostAmount += data.WinlostAmount;
                }
            }

            return result;
        }
        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId, int siteId)
        {
            return custInfoService.GetCustBankInfoLogList(custId, siteId);
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            return custInfoService.GetCustBonus(custId);
        }
        public TeleCSDData GetTeleCSD(string SiteIds, string Currency, long StartDate, long EndDate, string Deposit, string DormantUser, string CallOut, int PageNumber, int PageSize)
        {
            return custInfoService.GetTeleCSD(SiteIds, Currency, StartDate, EndDate, Deposit, DormantUser, CallOut, PageNumber, PageSize);
        }

        public int UpdateCustomerTele(int siteId,int custId,string Operator)
        {
            return custInfoService.UpdateCustomerTele(siteId, custId, Operator);
        }

        public IEnumerable<CustTeleExport> GetTeleCSDExportData(string SiteIds, string Currency, long StartDate, long EndDate, string Deposit, string DormantUser, string CallOut)
        {
            return custInfoService.GetTeleCSDExportData(SiteIds, Currency, StartDate, EndDate, Deposit, DormantUser, CallOut);
            
        }
    }
}
 