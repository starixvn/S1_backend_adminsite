using BackendSite.Extensions;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MemberQuery : IMemberQuery
    {
        private readonly IMemberService _memberService;
        private readonly IDeviceService _deviceService;
        private readonly ICacheHelper _cacheHelper;
        private readonly IMarketingService _marketingService;
        private readonly IAffiliateApiService _affiliateApi;
        private readonly ITagBLL _tagBll;
        private readonly Dictionary<string, SiteCloudFrontImageInfo> _siteConfig;
        private readonly PermissionBLL _permissionBll;
        private readonly AdminBLL _adminBll;
        private readonly Tools _tools;
        private readonly CustInfoBLL _custInfoBll;

        public MemberQuery(IMemberService memberService, IDeviceService deviceService, ICacheHelper cacheHelper, 
            Dictionary<string, SiteCloudFrontImageInfo> siteConfig, 
            PermissionBLL permissionBll,
            AdminBLL adminBll,
            IMarketingService marketingService, 
            IAffiliateApiService affiliateApi,
            Tools tools,
            CustInfoBLL custInfoBll, 
            ITagBLL tagBll)
        {
            _memberService = memberService;
            _deviceService = deviceService;
            _cacheHelper = cacheHelper;
            _permissionBll = permissionBll;
            _adminBll = adminBll;
            _marketingService = marketingService;
            _affiliateApi = affiliateApi;
            _tools = tools;
            _siteConfig = siteConfig;
            _custInfoBll = custInfoBll;
            _tagBll = tagBll;
        }

        public async Task<GetMemberListPageData<MemberList>> GetListAsync(GetTempMemberListRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var userName = string.IsNullOrEmpty(request.UserName) ? null : request.UserName;
            var currencyList = request.CurrencyId ?? new List<int>();
            var statusList = request.Status ?? new List<int>();
            var tagList = request.Tags ?? new List<int>();
            var vipList = request.VipLevel ?? new List<int>();
            var accountName = string.IsNullOrEmpty(request.AccountName) ? null : request.AccountName;
            var affiliateName = string.IsNullOrEmpty(request.AffiliateName) ? null : request.AffiliateName;
            var tpUserName = string.IsNullOrEmpty(request.TpUserName) ? null : request.TpUserName;
            var loginIp = string.IsNullOrEmpty(request.LoginIp) ? null : request.LoginIp;
            var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
            var phoneNo = string.IsNullOrEmpty(request.PhoneNo) ? null : request.PhoneNo;
            var bankAccountNumber = string.IsNullOrEmpty(request.BankAccountNumber) ? null : request.BankAccountNumber;
            var eWalletAccountNumber = string.IsNullOrEmpty(request.EWalletAccountNumber) ? null : request.EWalletAccountNumber;
            var cryptoAddress = string.IsNullOrEmpty(request.CryptoAddress) ? null : request.CryptoAddress;
            var affiliateCode = string.IsNullOrEmpty(request.AffiliateCode) ? null : request.AffiliateCode;
            var deviceId = string.IsNullOrEmpty(request.DeviceId) ? null : request.DeviceId;
            var transType = GetTransType(request.TransId);
            var userTypes = request.UserTypes != null && request.UserTypes.Any() ? request.UserTypes.Select(x => (int)x)
                : ((MemberTypeEnum[])Enum.GetValues(typeof(MemberTypeEnum))).Select(x => (int)x);

            var memberList = await _memberService.GetList(request.SiteId, currencyList, statusList, request.Identity,
                userName, tagList, vipList, accountName, affiliateName, tpUserName, loginIp, email, phoneNo, bankAccountNumber,
                eWalletAccountNumber, cryptoAddress,
                affiliateCode, transType, request.TransId, request.ShieldLevel, request.IovationLevel, deviceId, userTypes,
                request.DateType, startTime, endTime, request.PageNumber, request.PageSize, request.ExcludeTag);
            var pointLevelInfo = await _marketingService.GetPointLevelInfos(request.SiteId);

            var response = new GetMemberListPageData<MemberList>()
            {
                Data = memberList.Data.Select(a => new MemberList()
                {
                    SiteName = a.SiteName,
                    CurrencyId = a.CurrencyId,
                    IsoCode = a.IsoCode,
                    CustId = a.CustId,
                    UserName = a.UserName,
                    SabaId = a.SabaId,
                    AffiliateCode = a.AffCode,
                    AffiliateName = a.AffiliateName,
                    TagList = string.IsNullOrEmpty(a.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(a.TagList),
                    Identity = a.Identity,
                    UserType = a.UserType,
                    PointLevel = CalculatePointLevel(pointLevelInfo, a.CurrencyId, a.DepositCnt, a.WithdrawCnt, a.IsKyc, a.IsRisk, a.UserLevel),
                    Agent = a.Agent,
                    Status = a.Status,
                    IsTest = a.IsTest,
                    IP = a.IP,
                    LevelName = a.LevelName,
                    RegTime = a.RegTime.ToOffset(TimeSpan.Zero),
                    LastActiveTime = a.ReloadTime.ToOffset(TimeSpan.Zero),
                    LastTagUpdateTime = a.LastTagUpdateTime.ToOffset(TimeSpan.Zero),
                    LastStatusUpdateTime = a.LastStatusUpdateTime.ToOffset(TimeSpan.Zero),
                    Balance = a.Balance,
                    Bonus = a.Bonus,
                    DeviceScore = a.DeviceScore,
                    DeviceId = a.ShieldId
                }),
                PageNumber = memberList.PageNumber,
                TotalPages = memberList.TotalPages,
                RowCount = memberList.RowCount
            };

            if (!string.IsNullOrEmpty(request.UserName))
            {
                response.Data = SortByUserName(response.Data, request.UserName);
            }
            return response;
        }

        private IEnumerable<MemberList> SortByUserName(IEnumerable<MemberList> source, string userName)
        {
            // 先全部用UserName排序
            var result = source.OrderBy(i => i.UserName).ToList();
            // 檢查是否有完全符合的UserName
            var index = result.FindIndex(x => x.UserName == userName);
            if (index != -1)
            {
                var item = result[index];
                // 移除比對相符的資料
                result.RemoveAt(index);
                // 放到第一筆資料
                result.Insert(0, item);
            }

            return result;
        }

        public async Task<IEnumerable<MemberExportData>> GetExportDataAsync(ExportMemberRequest request)
        {
            var timespan = request.StartDate.Offset;

            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var userName = string.IsNullOrEmpty(request.UserName) ? null : request.UserName;
            var currencyList = request.CurrencyId ?? new List<int>();
            var statusList = request.Status ?? new List<int>();
            var tagList = request.Tags ?? new List<int>();
            var vipList = request.VipLevel ?? new List<int>();
            var accountName = string.IsNullOrEmpty(request.AccountName) ? null : request.AccountName;
            var affiliateName = string.IsNullOrEmpty(request.AffiliateName) ? null : request.AffiliateName;
            var tpUserName = string.IsNullOrEmpty(request.TpUserName) ? null : request.TpUserName;
            var loginIp = string.IsNullOrEmpty(request.LoginIp) ? null : request.LoginIp;
            var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
            var phoneNo = string.IsNullOrEmpty(request.PhoneNo) ? null : request.PhoneNo;
            var bankAccountNumber = string.IsNullOrEmpty(request.BankAccountNumber) ? null : request.BankAccountNumber;
            var eWalletAccountNumber = string.IsNullOrEmpty(request.EWalletAccountNumber) ? null : request.EWalletAccountNumber;
            var cryptoAddress = string.IsNullOrEmpty(request.CryptoAddress) ? null : request.CryptoAddress;
            var affiliateCode = string.IsNullOrEmpty(request.AffiliateCode) ? null : request.AffiliateCode;
            var deviceId = string.IsNullOrEmpty(request.DeviceId) ? null : request.DeviceId;
            var transType = GetTransType(request.TransId);
            var userTypes = request.UserTypes != null && request.UserTypes.Any() ? request.UserTypes.Select(x => (int)x)
                : ((MemberTypeEnum[])Enum.GetValues(typeof(MemberTypeEnum))).Select(x => (int)x);

            var memberList = await _memberService.GetExportData(request.SiteId, currencyList, statusList,
                request.Identity, userName, tagList, vipList, accountName, affiliateName, tpUserName, loginIp, email, phoneNo,
                bankAccountNumber, eWalletAccountNumber, cryptoAddress, affiliateCode, transType, request.TransId, request.ShieldLevel,
                request.IovationLevel, deviceId, userTypes, request.DateType, startTime, endTime, request.ExcludeTag);
            var allTagList = await _tagBll.GetAllTag();
            var data = memberList.Select(a => new MemberExportData()
            {
                SiteName = a.SiteName,
                UserName = a.UserName,
                CustId = a.CustId,
                Currency = a.IsoCode,
                BirthDate = a.BirthDate,
                BirthMonth = a.BirthMonth,
                BirthYear = a.BirthYear,
                IsTest = a.IsTest,
                IsKyc = a.IsKyc,
                IsRisk = a.IsRisk,
                SeamlessWallet = a.SeamlessWallet,
                BonusWallet = a.BonusWallet,
                Email = a.Email,
                RealName = a.RealName,
                CountryCode = a.CountryCode,
                AreaCode = a.AreaCode,
                PhoneNo = a.PhoneNo,
                LevelName = a.LevelName,
                Agent = a.Agent,
                RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero).ToOffset(timespan),
                Status = ((CustomerStatusEnum)a.Status).ToString(),
                SabaId = a.SabaId,
                SignupUrl = a.SignupUrl,
                AffiliateId = a.AffiliateId,
                AffiliateCode = a.AffiliateCode,
                LastActivityIp = a.LastActivityIp,
                Tag = GetTag(allTagList, a.TagList),
                UserType = a.UserType
            });
            if (request.ExportTransferWallet)
            {
                var exportDatas = new List<MemberExportData>();
                var tasks = data.Select(async x =>
                {
                    var exportData = await _custInfoBll.GetTransferWalletAsync(x);
                    exportDatas.Add(exportData);
                });
                await Task.WhenAll(tasks);
                //如果該名user的PtWallet,SeamlessWallet,BonusWallet 皆為0就不export該名user
                exportDatas.RemoveAll(x => x.PTWallet == 0 && x.SeamlessWallet == 0 && x.BonusWallet == 0);
                return exportDatas;
            }
            return data;
        }

        private int GetTransType(string transId)
        {
            if (string.IsNullOrEmpty(transId))
                return 0;
            // 提款
            if (transId.StartsWith("W"))
                return 1;
            // 存款
            if (transId.StartsWith("D"))
                return 2;
            // 轉帳
            if (transId.StartsWith("T") || transId.StartsWith("AT") || transId.StartsWith("BT"))
                return 3;
            return 0;
        }

        public async Task<CustomerBasicInfo> GetCustomerBasicInfoAsync(AuthData authData, int custId)
        {
            var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
            var isOnline = await _cacheHelper.KeyExistsAsync(custId.ToString(), RedisDataBase.Token);
            var custLineProfile = await _memberService.GetCustLineProfileAsync(custId, null);

            var loginUser = _adminBll.GetUserV2(authData.UserId, authData.SiteId);
            var hasPermission = _permissionBll.CheckPermission(loginUser, 22, 39);
            var email = customer.Email;
            var phoneNo = customer.PhoneNo;
            if (!hasPermission)
            {
                if (custLineProfile != null)
                {
                    custLineProfile.userId = _tools.MaskLineId(custLineProfile.userId);
                }

                email = _tools.MaskEmail(customer.Email);
                phoneNo = _tools.MaskPhone(customer.PhoneNo);
            }

            var customerBasicInfo = new CustomerBasicInfo()
            {
                CustId = customer.CustId,
                VipLevel = customer.VipLevel,
                LevelName = customer.LevelName,
                VipLogo = $"{_siteConfig[customer.SiteName].BannerImageUrl}/desktop/img/vip/vip{customer.VipLevel}.png",
                IsOnline = isOnline,
                UserName = customer.UserName,
                UserLang = customer.UserLang,
                AccountName = customer.AccountName,
                SabaId = customer.SabaId,
                PhoneVerified = customer.PhoneVerified,
                EmailVerified = customer.EmailVerified,
                ActStatus = customer.ActStatus,
                IsLineBinding = custLineProfile != null,
                CustLineProfile = custLineProfile,
                Status = customer.Status,
                EnableRebate = customer.EnableRebate,
                DepositPointLevel = customer.PointLevel,
                Currency = customer.CurrencyName,
                Identity = customer.Identity,
                AgentName = customer.Agent,
                SuperiorAffiliateId = customer.AffId,
                SuperiorAffiliateCode = customer.AffCode,
                RegisteredTime = customer.RegTime.ToOffset(TimeSpan.Zero),
                FirstDepositTime = customer.FirstDepositTime.ToOffset(TimeSpan.Zero),
                LastLoginTime = customer.LastLoginTime.ToOffset(TimeSpan.Zero),
                LastActiveTime = customer.LastDepositTime.ToOffset(TimeSpan.Zero),
                BirthDay = customer.BirthDay,
                BirthMonth = customer.BirthMonth,
                BirthYear = customer.BirthYear,
                Email = email,
                MobileNumber = $"({customer.AreaCode}) {phoneNo}",
                Address = AddressFormat.Format(customer.AddressLineSecond, customer.AddressLineFirst,
                    customer.AddressCity, customer.AddressPostCode, customer.AddressState, customer.AddressCountry),
                IsRisk = customer.IsRisk,
                IsKyc = customer.IsKyc,
                IsTest = customer.IsTest,
                TagList = customer.TagList == null
                    ? new List<CustomerTag>()
                    : await _memberService.GetCustomerTagsAsync(customer.TagList),
                PhoneRewardTime = customer.PhoneRewardTime.ToOffset(TimeSpan.Zero),
                EmailRewardTime = customer.EmailRewardTime.ToOffset(TimeSpan.Zero),
                PhoneVerifyTime = customer.PhoneVerifyTime.ToOffset(TimeSpan.Zero),
                EmailVerifyTime = customer.EmailVerifyTime.ToOffset(TimeSpan.Zero),
                MemberType = customer.MemberType,
                Exclusive = customer.Exclusive
            };
            if (!string.IsNullOrEmpty(customer.AffPlayerName))
            {
                var affInfo = await _affiliateApi.QueryAffInfo(customer.AffPlayerName, customer.CurrencyName, customer.SiteId);
                if (affInfo != null)
                {
                    affInfo.AffiliateCode = customer.AffiliateCode;
                    customerBasicInfo.AffInfo = affInfo;
                }
            }

            return customerBasicInfo;
        }
        public async Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustIdAsync(int custId)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(custId);

            if (custInfo.SiteId == 1)
            {
                var iovationList = await _deviceService.GetCustIovationList(custId);
                return iovationList.Select(a => new MemberLoginDevice()
                {
                    DeviceId = a.DeviceId,
                    Risk = a.Iovation,
                    Score = a.Score,
                    Frequency = a.Frequency,
                    DeviceType = a.DeviceType,
                    AssociateCount = a.AssociateCount,
                    LastUpdateTime = a.LastUpdateTime
                }).OrderByDescending(x => x.LastUpdateTime);
            }

            var shield = await _deviceService.GetCustShieldRiskInfoList(custId);
            return shield.Select(a => new MemberLoginDevice()
            {
                DeviceId = a.DeviceId,
                Risk = a.DeviceScore < 75 ? "High Risk" : "Low Risk",
                Score = a.DeviceScore,
                Frequency = a.Frequency,
                DeviceType = a.Platform,
                AssociateCount = a.AssociateCount,
                LastUpdateTime = a.LastUpdateTime
            }).OrderByDescending(x => x.LastUpdateTime);
        }

        public async Task<IEnumerable<MemberLoginDeviceFPS>> GetFPSDeviceListByCustIdAsync(int custId)
        {
            var fpsList = await _deviceService.GetCustFPSList(custId);
            return fpsList.Select(x => new MemberLoginDeviceFPS()
            {
                DeviceId = x.DeviceId,
                RiskLevel = x.RiskLevel,
                DeviceType = x.DeviceType,
                Frequency = x.Frequency,
                AssociateCount = x.AssociateCount,
                LastUpdateTime = x.LastUpdateTime
            }).OrderByDescending(x => x.LastUpdateTime);
        }

        public async Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId)
        {
            return await _memberService.GetCustomerIpListByCustIdAsync(custId);
        }

        public async Task<GetLoginLogResponse> GetLoginLogAsync(int custId, DateTime startTime, DateTime endTime, int pageSize, int pageNumber)
        {
            var data = await _memberService.GetLoginLogAsync(custId, startTime, endTime, pageSize, pageNumber);
            var response = new GetLoginLogResponse()
            {
                LoginLog = data.LoginLog.Select(a => new MemberLoginLog()
                {
                    Id = a.Id,
                    LoginTime = new DateTimeOffset(a.LoginTime, TimeSpan.Zero),
                    LoginIp = a.LoginIp,
                    FromUrl = a.FromUrl,
                    Location = a.Location,
                    UserAgent = a.UserAgent
                }),
                PageNumber = data.PageNumber,
                TotalPages = data.TotalPages,
                RowCount = data.RowCount
            };
            return response;
        }
        public async Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId)
        {
            return await _memberService.GetCustomerBalanceInfoAsync(custId);
        }

        public async Task<IEnumerable<CustomerAssociateBankInfo>> GetCustomerAssociateBankUserAsync(int custId)
        {
            return await _memberService.GetCustomerAssociateBankUserAsync(custId);
        }

        public async Task<IEnumerable<CustRealNameChangeLog>> GetCustRealNameChangeLogAsync(int custId)
        {
            return await _memberService.GetCustRealNameChangeLogAsync(custId);
        }

        public async Task<CustPromotionWhiteList> GetCustPromotionWhiteListAsync(int custId)
        {
            var result = await _memberService.GetCustPromotionWhiteListAsync(custId);
            if (result == null)
            {
                var customer = _custInfoBll.GetCustInfoWithCache(custId);
                result = new CustPromotionWhiteList()
                {
                    CustId = custId,
                    IpEnable = false,
                    DeviceEnable = false,
                    RealNameEnable = false,
                    SiteId = customer.SiteId,
                    UserName = customer.UserName,
                    Operator = string.Empty
                };
            }

            return result;
        }

        public async Task<IEnumerable<PromotionReceiveFailLog>> GetPromotionReceiveFailLogAsync(int custId, DateTime startTime, DateTime endTime)
        {
            return await _memberService.GetPromotionReceiveFailLogAsync(custId, startTime, endTime);
        }

        private int CalculatePointLevel(IEnumerable<PointLevelInfo> pointLevelInfos, int currencyId, int depositSum, int withdrawSum, bool isKyc, bool isRisk, int userLevel)

        {
            var info = pointLevelInfos
                .OrderByDescending(a => a.PointLevel)
                .FirstOrDefault(a =>
                    a.CurrencyId == currencyId && a.DepositCnt <= depositSum &&
                    a.WithdrawalCnt <= withdrawSum && a.IsKyc == isKyc);
            var pointLevel = info?.PointLevel ?? 1;
            return isRisk ? Math.Min(userLevel, pointLevel) : Math.Max(userLevel, pointLevel);
        }
        private DateTimeOffset? Convert(DateTime? dateTime, TimeSpan ts)
        {
            if (dateTime.HasValue)
                return new DateTimeOffset(dateTime.Value, ts);
            return null;
        }
        private string GetTag(IEnumerable<Tag> allTagList, string tagList)
        {
            List<string> temp = new List<string>();
            if (string.IsNullOrWhiteSpace(tagList))
            {
                return string.Empty;
            }

            JsonConvert.DeserializeObject<List<int>>(tagList).ToList().ForEach(x =>
            {
                var tagName = allTagList.FirstOrDefault(y => y.Id == x)?.TagName;
                if (!string.IsNullOrWhiteSpace(tagName))
                {
                    temp.Add(tagName);
                }
            });

            return string.Join(",", temp);
        }

        public async Task<IEnumerable<CustTpGameTypeSettingForFrontendUse>> GetCustTpGameTypeSettingAsync(int custId, IEnumerable<CustTpGameTypeSettingGetType> getDataTypes)
        {
            var types = getDataTypes.Select(x => _custInfoBll.ConvertCustTpGameTypeSettingType(x));
            var settingResult = await _memberService.GetCustTpGameTypeSettingAsync(custId);

            foreach (var type in getDataTypes)
            {
                switch (type)
                {
                    case CustTpGameTypeSettingGetType.IsPlay:
                        settingResult = settingResult.Where(x => x.NoPlay == false);
                        break;
                    case CustTpGameTypeSettingGetType.NoPlay:
                        settingResult = settingResult.Where(x => x.NoPlay == true);
                        break;
                    case CustTpGameTypeSettingGetType.IsShow:
                        settingResult = settingResult.Where(x => x.NoShow == false);
                        break;
                    case CustTpGameTypeSettingGetType.NoShow:
                        settingResult = settingResult.Where(x => x.NoShow == true);
                        break;
                }
            }
            return settingResult;
        }

    }
}