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
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class CustInfoBLL
    {
        private readonly CustInfoService _custInfoService;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly IMemoryCache _cache;
        private readonly TpBLL _tpBll;
        private readonly WalletService _walletService;
        private readonly Tools _tools;
        private readonly CodeGenerator _codeGenerator;
        private readonly PromotionService _promotionService;
        private readonly RollOverService _rolloverService;
        private readonly CustTurnoverService _custTurnoverService;
        private readonly BonusInfoService _bonusInfoService;
        public CustInfoBLL(IServiceProvider provider)
        {
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _cache = provider.GetRequiredService<IMemoryCache>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _walletService = provider.GetRequiredService<WalletService>();
            _tools = provider.GetRequiredService<Tools>();
            _codeGenerator = provider.GetRequiredService<CodeGenerator>();
            _promotionService = provider.GetRequiredService<PromotionService>();
            _rolloverService = provider.GetRequiredService<RollOverService>();
            _custTurnoverService = provider.GetRequiredService<CustTurnoverService>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
        }

        public Customer GetCustInfoWithCache(int custId)
        {
            var key = $"CustInfo_{custId}";
            var customer = _cache.Get<Customer>(key);
            if (customer == null)
            {
                customer = _custInfoService.GetCustInfo(custId);
                if (customer != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));
                    _cache.Set(key, customer, cacheEntryOptions);
                }
            }

            return customer;
        }
        public int GetVipLevelWithCache(int custId)
        {
            var key = $"VipLevelCache_{custId}";
            var vipLevelCache = _cache.Get<int?>(key) ?? -1;
            if (vipLevelCache != -1)
            {
                return vipLevelCache;
            }

            vipLevelCache = _custInfoService.GetVipLevel(custId);
            if (vipLevelCache != 0)
            {
                _cache.Set(key, vipLevelCache, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
            }

            return vipLevelCache;
        }
        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string accountName,
            string loginIp,
            string transId,
            string tpUserName,
            string email,
            string phoneNo,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            int pageSize,
            int? affId,
            IEnumerable<int> selectCurrency,
            string bankAccountNumber,
            IEnumerable<int> selectStatus,
            ref int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser().SelectMany(x => new[] { x.SiteId });
            var currencyList = new List<int>();
            _serverInfoBll.GetSiteListByLoginUser().ToList().ForEach(x =>
            {
                currencyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            var custInfoList = _custInfoService.GetCustInfoList(selectSite, birDate, birMonth, birYear, userName, accountName, loginIp, transId, tpUserName, email, phoneNo, startDate, endDate, pageSize, affId, selectCurrency, bankAccountNumber, selectStatus, ref pageNumber, out totalPages, out rowCount, JsonConvert.SerializeObject(siteList), JsonConvert.SerializeObject(currencyList.Select(x => x).Distinct().ToList())).ToList();
            if (custInfoList.Count > 0)
            {
                custInfoList.ForEach(x =>
                {
                    x.ITagList = string.IsNullOrWhiteSpace(x.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.TagList);
                });
            }
            return custInfoList;
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string AccountName,
            string LoginIp,
            string transId,
            string TpUserName,
            string Email,
            string PhoneNo,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            int? AffId,
            IEnumerable<int> selectCurrency,
            string BankAccountNumber,
            IEnumerable<int> selectStatus)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });

            var currncyList = new List<int>();
            _serverInfoBll.GetSiteListByLoginUser().ToList().ForEach(x =>
            {
                currncyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            var result = _custInfoService.GetCustInfoExportList(selectSite, birDate, birMonth, birYear, userName, AccountName, LoginIp, transId, TpUserName, Email, PhoneNo, startDate, endDate, JsonConvert.SerializeObject(siteList), AffId, selectCurrency, BankAccountNumber, JsonConvert.SerializeObject(currncyList.Select(x => x).Distinct().ToList()), selectStatus);


            return result;
        }
        public async Task<CustInfoDetail> GetCustInfoDetailAsync(int custId, string userName)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = _custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.LastDepositTime = custInfoDetail.LastDepositTimeRecord == null ? "N/A" : Convert.ToDateTime(custInfoDetail.LastDepositTimeRecord).ToUniversalTime().AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.LastWithdrawlTime = custInfoDetail.LastWithdrawalTimeRecord == null ? "N/A" : Convert.ToDateTime(custInfoDetail.LastWithdrawalTimeRecord).ToUniversalTime().AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.LastBettingTime = custInfoDetail.LastTpBetRecordTimeRecord == null ? "N/A" : Convert.ToDateTime(custInfoDetail.LastTpBetRecordTimeRecord).ToUniversalTime().AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                custInfoDetail.FirstDepositTime = custInfoDetail.FirstDepositTimeRecord == null ? "N/A" : Convert.ToDateTime(custInfoDetail.FirstDepositTimeRecord).ToUniversalTime().AddHours(8).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                custInfoDetail.tpWallet = await _tpBll.GetTpWalletAsync(custInfoDetail.CustId);
                custInfoDetail.promotions = _promotionService.GetPromotionListByCustId(custInfoDetail.CustId);
                if (custInfoDetail.promotions != null)
                {
                    foreach (var promotion in custInfoDetail.promotions)
                    {
                        if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                        {
                            promotion.custRollOver = _rolloverService.GetCustRollOver(custInfoDetail.CustId, promotion.BonusCode);
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
            return _custInfoService.GetCustInfos(custId, userName, siteId);
        }
        
        public bool SendMessageBatch(BatchPersonalMsg req)
        {
            return _custInfoService.SendMessageBatch(req);
        }

        public decimal GetBalance(int custId)
        {
            return _walletService.GetBalance(custId);
        }
        public int SetCustStatusSwitch(int custId, int Op)
        {
            return _custInfoService.SetCustStatusSwitch(custId, Op);
        }
        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, string Operator)
        {
            return _custInfoService.SetCustInfo(custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, Operator);
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _custInfoService.GetLoginLogByCust(CustId, startDate, endDate, Math.Abs(pageSize), (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), out totalPages, out rowCount);
        }
        public bool SetResetPwd(int CustId, out string NewPassword)
        {
            _codeGenerator.Maximum = 8;
            _codeGenerator.Minimum = 6;
            _codeGenerator.RepeatCharacters = true;
            NewPassword = _codeGenerator.Generate();
            var md5NewPassword = _tools.MD5Encrypt(NewPassword);
            return _custInfoService.SetResetPwd(CustId, md5NewPassword);
        }
        public bool ResetProfile(int CustId)
        {
            return _custInfoService.ResetProfile(CustId);
        }
        public bool ResetCustBankInfo(int CustId)
        {
            return _custInfoService.ResetCustBankInfo(CustId);
        }
        public bool ResetCustUSDTInfo(int CustId)
        {
            return _custInfoService.ResetCustUSDTInfo(CustId);
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            return _custInfoService.GetWithdrawalAvailableBalance(custId);
        }
        public int CancelBonusByBonusId(int bonusId)
        {
            var statusList = new List<int>
            {
                (int) BonusStatusType.Initial,
                (int) BonusStatusType.Running,
                (int) BonusStatusType.TrasferOut,
            };
            return _bonusInfoService.CancelBonusByBonusId(false, bonusId, JsonConvert.SerializeObject(statusList), out var CustId);
        }

        public List<CustTurnoverRes> GetCustTurnover(int CustId,
            string GameTypes,
            DateTimeOffset StartTime,
            DateTimeOffset EndTime)
        {
            var datas = _custTurnoverService.GetCustTurnover(CustId,
                GameTypes,
                StartTime,
                EndTime);

            var result = new List<CustTurnoverRes>();

            foreach (var data in datas)
            {
                var item = result.FirstOrDefault(x => x.GameType == data.GameType);
                if (item == null)
                {
                    result.Add(new CustTurnoverRes
                    {
                        UserName = data.UserName,
                        GameType = data.GameType,
                        GameName = data.GameName,
                        StakeSettled = data.SettlementType == 1 ? data.Stake : 0,
                        StakeNotRollover = data.SettlementType == 3 ? data.Stake : 0,
                        WinlostAmount = data.WinlostAmount
                    });
                }
                else
                {
                    item.StakeSettled += data.SettlementType == 1 ? data.Stake : 0;
                    item.StakeNotRollover += data.SettlementType == 3 ? data.Stake : 0;
                    item.WinlostAmount += data.WinlostAmount;
                }
            }

            return result;
        }
        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustBankInfoLogList(custId,
                 Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }
        public IEnumerable<CustCryptoWalletLog> GetCustCryptoWalletLogList(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustCryptoWalletLogList(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount); ;
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            return _custInfoService.GetCustBonus(custId);
        }
        public TeleCSDData GetTeleCSD(string siteIds, string currency, DateTimeOffset startDate, DateTimeOffset endDate, int deposit, int dormantUser, int callOut, int pageNumber, int pageSize)
        {
            return _custInfoService.GetTeleCSD(siteIds, currency, startDate, endDate, deposit, dormantUser, callOut, pageNumber, pageSize);
        }

        public int UpdateCustomerTele(int siteId, int custId, string @operator)
        {
            return _custInfoService.UpdateCustomerTele(siteId, custId, @operator);
        }

        public IEnumerable<CustTeleExport> GetTeleCSDExportData(string siteIds, string currency, DateTimeOffset startDate, DateTimeOffset endDate, int deposit, int dormantUser, int callOut)
        {
            return _custInfoService.GetTeleCSDExportData(siteIds, currency, startDate, endDate, deposit, dormantUser, callOut);
        }

        public PromotionListData GetPromotionListByCustIdAndDate(int custId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            var result = _promotionService.GetPromotionListByCustIdAndDate(custId, StartDate, EndDate, PageNumber, PageSize);
            if (result.PromotionList != null)
            {
                foreach (var promotion in result.PromotionList)
                {
                    if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                    {
                        promotion.custRollOver = _rolloverService.GetCustRollOver(custId, promotion.BonusCode);
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
            return _custInfoService.GetCustPhotoId(custId);
        }
        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            return _custInfoService.SetCustPhotoId(custId, siteId, picFileName, photoIdPic);
        }
        public bool DelCustPhotoId(int custId)
        {
            return _custInfoService.DelCustPhotoId(custId);
        }

        #endregion

        #region Customer Note

        public bool AddCustNote(int custId, string Note, string Operator)
        {
            return _custInfoService.AddCustNote(custId, Note, Operator);
        }
        public IEnumerable<CustNote> GetCustNotes(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustNotes(custId,
                 Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }
        public bool SetCustNotes(int noteId, string note, int type, string Operator)
        {
            return _custInfoService.SetCustNotes(noteId, note, type, Operator);
        }

        #endregion

        #region Customer Status Update

        public bool SetCustStatus(string menu, int custId, int status, string reason, string action, string user)
        {
            var result = false;
            var mainInfo = new Dictionary<string, dynamic>();
            var noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", action);
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using var scope = new TransactionScope();

            #region Update Status

            result = _custInfoService.SetCustStatus(custId, status);

            if (!result)
            {
                scope.Dispose();

                return false;
            };

            if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Update, mainInfo, user) <= 0)
            {
                scope.Dispose();

                return false;
            }

            #endregion

            if (result)
            {
                #region Insert Reason For Update Status

                if (!_custInfoService.AddCustNote(custId, action + " Reason : " + reason, user))
                {
                    scope.Dispose();

                    return false;
                }

                if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                {
                    scope.Dispose();

                    return false;
                }

                #endregion
            }

            scope.Complete();

            return result;

        }

        /// <summary>
        /// 刪除在線使用者資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteOnUserData(string menu, int custId, string reason, string user)
        {
            var result = 0;
            var mainInfo = new Dictionary<string, dynamic>();
            var noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", "Kick Out");
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using var scope = new TransactionScope();

            #region Kick Out User

            result = _custInfoService.DeleteOnUserData(custId);

            if (result <= 0)
            {
                scope.Dispose();

                return 0;
            };

            if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Delete, mainInfo, user) <= 0)
            {
                scope.Dispose();

                return 0;
            }

            #endregion

            if (result > 0)
            {
                #region Insert Reason For Kick Out

                if (!_custInfoService.AddCustNote(custId, "Kick Out Reason : " + reason, user))
                {
                    scope.Dispose();

                    return 0;
                }

                if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                {
                    scope.Dispose();

                    return 0;
                }

                #endregion
            }

            scope.Complete();

            return result;
        }

        #endregion

        #region TagMamagement
        public int EditTagManagement(int id, string tagName, string tagColour, string description, string @operator)
        {
            return _custInfoService.EditTagManagement(id, tagName, tagColour, description, @operator);
        }

        public GetTagDataResult GetTagData(int status, int pageNumber, int pageSize)
        {
            return _custInfoService.GetTagData(status, pageNumber, pageSize);
        }

        public int SetTagStatus(int id, string @operator)
        {
            return _custInfoService.SetTagStatus(id, @operator);
        }

        public IEnumerable<TagData> GetAllTag()
        {
            return _custInfoService.GetAllTag();
        }

        public int SetCustTag(int custid, IEnumerable<int> SelectTag, string Operator)
        {
            return _custInfoService.SetCustTag(custid, SelectTag);
        }
        public CustInfoDetail GetCustData(int custId, string userName)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = _custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.ITagList = string.IsNullOrWhiteSpace(custInfoDetail.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(custInfoDetail.TagList);
            }

            return custInfoDetail;
        }
        #endregion

        public int SetEnableDisablePromotion(int custId, int enablePromotion, string Operator)
        {
            var result = 1;

            using var scope = new TransactionScope();
            if (!_custInfoService.SetEnableDisablePromotion(custId, enablePromotion))
            {
                scope.Dispose();

                return 0;
            }
            if (!_custInfoService.AddCustNote(custId, enablePromotion == 0 ? "Disable Promotion" : "Enable Promotion", Operator))
            {
                scope.Dispose();

                return 0;
            }

            scope.Complete();
            return result;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return _custInfoService.GetAllSiteVipLevelList();
        }

        public RegLiveStatusResult GetRegLiveStatusData(IEnumerable<int> selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            return _custInfoService.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType);
        }

        public int SetRegLiveStatus(int custId, int status, int siteId, string @operator)
        {
            return _custInfoService.SetRegLiveStatus(custId, status, siteId, @operator);
        }

        public IEnumerable<RegLiveStatusNotes> GetRegLiveStatusNotes(int custId)
        {
            return _custInfoService.GetRegLiveStatusNotes(custId);
        }

        public int AddRegLiveStatusNote(int custId, string note, string @operator)
        {
            return _custInfoService.AddRegLiveStatusNote(custId, note, @operator);
        }

        public int DeleteRegLiveStatusNote(int custId, int id)
        {
            return _custInfoService.DeleteRegLiveStatusNote(custId, id);
        }

        public int EditRegLiveStatusNote(int custId, int id, string note, string @operator)
        {
            return _custInfoService.EditRegLiveStatusNote(custId, id, note, @operator);
        }

        public IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(string siteIds, int currency, int depositStatus, int status, string user, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var list = new List<RegLiveStatusExport>();
            var result = _custInfoService.GetExportRegLiveStatus(siteIds, currency, depositStatus, status, user, sort, startDate, endDate, affiliatesCode, agentCodeType);
            result.ToList().ForEach(x =>
            {
                list.Add(new RegLiveStatusExport()
                {
                    Site = x.SiteName,
                    Currency = x.Currency,
                    UserName = x.UserName,
                    FullName = x.AccountName,
                    PhoneNo = x.PhoneNo,
                    Verify = Enum.GetName(typeof(PhoneVerifiedEnum), x.PhoneVerified),
                    RegTime = new DateTimeOffset(x.RegTime.DateTime, TimeSpan.Zero),
                    DepositStatus = Enum.GetName(typeof(ActStatusEnum), x.ActStatus),
                    Status = Enum.GetName(typeof(RegLiveStatusType), x.Status),
                    AffCode = x.AffCode,
                    Agent = x.Agent
                });
            });
            return list;
        }

        public async Task<CustomerForRisk> GetCustId(string userName, int siteId)
        {
            return await _custInfoService.GetCustId(userName, siteId);
        }

        public CustomerSwitchStatus GetCustSwitchStatus(int custId)
        {
            return _custInfoService.GetCustSwitchStatus(custId);
        }

        public CustProfileReq GetCustProfile(int custId)
        {
            return _custInfoService.GetCustProfile(custId);
        }
    }
}