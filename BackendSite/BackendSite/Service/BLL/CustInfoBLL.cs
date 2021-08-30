using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

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
        private readonly BonusInfoService _bonusInfoService;
        public CustInfoBLL(IMemoryCache cache,
            WalletService walletService,
            CustInfoService custInfoService,
            ServerInfoBLL serverInfoBLL,
            TpBLL tpBLL,
            Tools tools,
            CodeGenerator codeGenerator,
            PromotionService promotionService,
            RollOverService rolloverService,
            CustTurnoverService custTurnoverService,
            BonusInfoService bonusInfoService)
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
            _bonusInfoService = bonusInfoService;
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
        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, int pageSize, int? affId, IEnumerable<int> selectCurrency, string bankAccountNumber, IEnumerable<int> selectstatus, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            List<int> currncyList = new List<int>();
            serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x =>
            {
                currncyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            var CustInfoList = custInfoService.GetCustInfoList(selectSite, birDate, birMonth, birYear, userName, AccountName, LoginIp, transId, TpUserName, Email, PhoneNo, startDate, endDate, pageSize, affId, selectCurrency, bankAccountNumber, selectstatus, ref pageNumber, out totalPages, out rowCount, JsonConvert.SerializeObject(siteList), JsonConvert.SerializeObject(currncyList.Select(x => x).Distinct().ToList())).ToList();
            if (CustInfoList.Count() > 0)
            {
                CustInfoList.ForEach(x =>
                {
                    x.ITagList = string.IsNullOrWhiteSpace(x.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.TagList);
                });
            }
            return CustInfoList;
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, int? AffId, IEnumerable<int> selectCurrency, string BankAccountNumber, IEnumerable<int> selectStatus)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });

            List<int> currncyList = new List<int>();
            serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x =>
            {
                currncyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            return custInfoService.GetCustInfoExportList(selectSite, birDate, birMonth, birYear, userName, AccountName, LoginIp, transId, TpUserName, Email, PhoneNo, startDate, endDate, JsonConvert.SerializeObject(siteList), AffId, selectCurrency, BankAccountNumber, JsonConvert.SerializeObject(currncyList.Select(x => x).Distinct().ToList()), selectStatus);
        }

        public async Task<CustInfoDetail> GetCustInfoDetailAsync(int custId, string userName)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.LastDepositTime = custInfoDetail.LastDepositTimeRecord == null ? "N/A" : custInfoDetail.LastDepositTimeRecord?.AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.LastWithdrawlTime = custInfoDetail.LastWithdrawalTimeRecord == null ? "N/A" : custInfoDetail.LastWithdrawalTimeRecord?.AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.LastBettingTime = custInfoDetail.LastTpBetRecordTimeRecord == null ? "N/A" : custInfoDetail.LastTpBetRecordTimeRecord?.AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.FirstDepositTime = custInfoDetail.FirstDepositTimeRecord == null ? "N/A" : custInfoDetail.FirstDepositTimeRecord?.AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

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
                            promotion.custRollOver = new CustRollOver() { RollOver = 0, TurnOver = 0 };
                        };

                    }
                }
                custInfoDetail.ITagList = string.IsNullOrWhiteSpace(custInfoDetail.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(custInfoDetail.TagList);
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
            var statusList = new List<int>
            {
                (int) BonusStatusType.Initial,
                (int) BonusStatusType.Running,
                (int) BonusStatusType.TrasferOut,
            };
            return _bonusInfoService.CancelBonusByBonusId(false, bonusId, JsonConvert.SerializeObject(statusList), out int CustId);
        }

        public List<CustTurnover> GetCustTurnover(int CustId, string GameTypes, long StartTime, long EndTime)
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
        public TeleCSDData GetTeleCSD(string SiteIds, string Currency, long StartDate, long EndDate, int Deposit, int DormantUser, int CallOut, int PageNumber, int PageSize)
        {
            return custInfoService.GetTeleCSD(SiteIds, Currency, StartDate, EndDate, Deposit, DormantUser, CallOut, PageNumber, PageSize);
        }

        public int UpdateCustomerTele(int siteId, int custId, string Operator)
        {
            return custInfoService.UpdateCustomerTele(siteId, custId, Operator);
        }

        public IEnumerable<CustTeleExport> GetTeleCSDExportData(string SiteIds, string Currency, long StartDate, long EndDate, int Deposit, int DormantUser, int CallOut)
        {
            return custInfoService.GetTeleCSDExportData(SiteIds, Currency, StartDate, EndDate, Deposit, DormantUser, CallOut);
        }

        public PromotionListData GetPromotionListByCustIdAndDate(int custId, long StartDate, long EndDate, int PageNumber, int PageSize)
        {
            var result = promotionService.GetPromotionListByCustIdAndDate(custId, StartDate, EndDate, PageNumber, PageSize);
            if (result.PromotionList != null)
            {
                foreach (var promotion in result.PromotionList)
                {
                    if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                    {
                        promotion.custRollOver = rolloverService.GetCustRollOver(custId, promotion.BonusCode);
                        if (promotion.BonusInfoStatus == 5)
                        {
                            promotion.BonusInfoStatus = 1;
                        }
                    }
                    else
                    {
                        promotion.custRollOver = new CustRollOver() { RollOver = 0, TurnOver = 0 };
                    };

                }
            }
            return result;
        }

        #region

        #endregion

        #region Customer Photo

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

        #endregion

        #region Customer Note

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

        #endregion

        #region Customer Status Update

        public bool SetCustStatus(string menu, int custId, int status, string reason, string action, string user)
        {
            bool result = false;
            Dictionary<string, dynamic> mainInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", action);
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using (TransactionScope scope = new TransactionScope())
            {
                #region Update Status

                result = custInfoService.SetCustStatus(custId, status);

                if (!result)
                {
                    scope.Dispose();

                    return false;
                };

                if (serverInfoBLL.AdminOperateLog(null, custId, menu, ActionType.Update, mainInfo, user) <= 0)
                {
                    scope.Dispose();

                    return false;
                }

                #endregion

                if (result)
                {
                    #region Insert Reason For Update Status

                    if (!custInfoService.AddCustNote(custId, action + " Reason : " + reason, user))
                    {
                        scope.Dispose();

                        return false;
                    }

                    if (serverInfoBLL.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                    {
                        scope.Dispose();

                        return false;
                    }

                    #endregion
                }

                scope.Complete();
            }

            return result;

        }

        /// <summary>
        /// 刪除在線使用者資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteOnUserData(string menu, int custId, string reason, string user)
        {
            int result = 0;
            Dictionary<string, dynamic> mainInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", "Kick Out");
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using (TransactionScope scope = new TransactionScope())
            {
                #region Kick Out User

                result = custInfoService.DeleteOnUserData(custId);

                if (result <= 0)
                {
                    scope.Dispose();

                    return 0;
                };

                if (serverInfoBLL.AdminOperateLog(null, custId, menu, ActionType.Delete, mainInfo, user) <= 0)
                {
                    scope.Dispose();

                    return 0;
                }

                #endregion

                if (result > 0)
                {
                    #region Insert Reason For Kick Out

                    if (!custInfoService.AddCustNote(custId, "Kick Out Reason : " + reason, user))
                    {
                        scope.Dispose();

                        return 0;
                    }

                    if (serverInfoBLL.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                    {
                        scope.Dispose();

                        return 0;
                    }

                    #endregion
                }

                scope.Complete();
            }

            return result;
        }

        #endregion

        #region TagMamagement
        public int EditTagMamagement(int id, string tagName, string tagColour, string description, string Operator)
        {
            return custInfoService.EditTagMamagement(id, tagName, tagColour, description, Operator);
        }

        public GetTagDataResult GetTagData(int status, int pageNumber, int pageSize)
        {
            return custInfoService.GetTagData(status, pageNumber, pageSize);
        }

        public int SetTagStatus(int Id, string Operator)
        {
            return custInfoService.SetTagStatus(Id, Operator);
        }

        public IEnumerable<TagData> GetAllTag()
        {
            return custInfoService.GetAllTag();
        }

        public int SetCustTag(int custid, IEnumerable<int> SelectTag, string Operator)
        {
            return custInfoService.SetCustTag(custid, SelectTag);
        }
        public CustInfoDetail GetCustData(int custId, string userName)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.ITagList = string.IsNullOrWhiteSpace(custInfoDetail.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(custInfoDetail.TagList);
            }

            return custInfoDetail;
        }
        #endregion

        public int SetEnableDisablePromotion(int custId, int enablePromotion, string Operator)
        {
            int result = 1;

            using (TransactionScope scope = new TransactionScope())
            {
                if (!custInfoService.SetEnableDisablePromotion(custId, enablePromotion))
                {
                    scope.Dispose();

                    return 0;
                }
                if (!custInfoService.AddCustNote(custId, enablePromotion == 0 ? "Disable Promotion" : "Enable Promotion", Operator))
                {
                    scope.Dispose();

                    return 0;
                }

                scope.Complete();
            }
            return result;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return custInfoService.GetAllSiteVipLevelList();
        }

        public RegLiveStatusResult GetRegLiveStatusData(IEnumerable<int> selectSite, int currency,int depositStatus,int status,string username,int pagenumber,int pagesize,int sort, long StartDate, long EndDate)
        {
            return custInfoService.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pagenumber, pagesize, sort, StartDate, EndDate);
        }

        public int SetRegLiveStatus(int custid,int status,int siteid,string Operator)
        {
            return custInfoService.SetRegLiveStatus(custid, status, siteid, Operator);
        }

        public IEnumerable<RegLiveStatusNotes> GetRegLiveStatusNotes(int custId)
        {
            return custInfoService.GetRegLiveStatusNotes(custId);
        }

        public int AddRegLiveStatusNote(int custid,string note, string Operator)
        {
            return custInfoService.AddRegLiveStatusNote(custid, note, Operator);
        }

        public int DeleteRegLiveStatusNote(int custid,int id)
        {
            return custInfoService.DeleteRegLiveStatusNote(custid, id);
        }

        public int EditRegLiveStatusNote(int custid,int id, string note, string Operator)
        {
            return custInfoService.EditRegLiveStatusNote(custid, id, note, Operator);
        }

        public IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(string SelectSite, int Currency, int DepositStatus, int Status, string User, int Sort, long StartDate, long EndDate)
        {
            List<RegLiveStatusExport> list = new List<RegLiveStatusExport>();
            var result =  custInfoService.GetExportRegLiveStatus(SelectSite, Currency, DepositStatus, Status, User, Sort, StartDate, EndDate);
            result.ToList().ForEach(x =>
            {
                list.Add(new RegLiveStatusExport()
                {
                    Site = x.SiteName,
                    Currency = x.Currency,
                    UserName= x.UserName,
                    FullName = x.AccountName,
                    PhoneNo = x.PhoneNo,
                    Verify = Enum.GetName(typeof(VerifyType), x.PhoneVerified),
                    RegTime = x.RegTime.AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                    DepositStatus = Enum.GetName(typeof(DepositStatusType), x.ActStatus),
                    Status = Enum.GetName(typeof(RegLiveStatusType), x.Status),
                });
            });
            return list;
        }

        public async Task<CustomerForRisk> GetCustId(string userName, int siteId)
        {
            return await custInfoService.GetCustId(userName, siteId);
        }
    }
}