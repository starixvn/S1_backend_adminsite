using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationBLL : ILiveRegistrationBLL
    {
        private readonly ILiveRegistrationService _registrationService;
        private readonly IMemberService _memberService;
        private readonly ServerInfoBLL _serverInfoBll;

        public LiveRegistrationBLL(ILiveRegistrationService registrationService, IMemberService memberService,ServerInfoBLL serverInfoBll)
        {
            _registrationService = registrationService;
            _memberService = memberService;
            _serverInfoBll = serverInfoBll;
        }

        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(GetRegLiveStatusRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var timezone = request.StartDate.Offset;
            var custList = new List<int>();

            if (request.DateType == LiveRegistrationDateTypeEnum.CallRecordDate ||
                !string.IsNullOrEmpty(request.UserName))
            {
                custList = await GetQueryCustId(request.DateType, request.SiteIds, request.SiteId, startTime, endTime, request.UserName);
                if (custList == null || custList.Count == 0)
                    return new LiveRegistrationResponse()
                    {
                        LiveRegistration = new List<LiveRegistration>(),
                        PageNumber = 1,
                        TotalPages = 0,
                        RowCount = 0
                    };
            }
            
            var (dormantFrequency, dormantType) = GetDormant(request.DormantUser);

            var orderByProperty = string.IsNullOrEmpty(request.OrderByProperty) ? "RegTime" : request.OrderByProperty;
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);

            var searchResult = await _registrationService.GetRegLiveStatusData(JsonConvert.SerializeObject(multiSiteRequest),
                request.DepositStatus, request.Status, custList, (int)request.DateType, startTime, endTime,
                request.AffiliatesCode, request.AgentCodeType, dormantType, dormantFrequency, request.CallOutStatus,
                orderByProperty, (int)request.OrderByDirection, request.PageNumber, request.PageSize);

            var regLiveStatus = searchResult.RegLiveStatusData.Select(a =>
                new LiveRegistration()
                {
                    SiteId = a.SiteId,
                    CustId = a.CustId,
                    Currency = a.Currency,
                    UserName = a.UserName,
                    AccountName = a.AccountName,
                    Agent = a.Agent,
                    AffiliatesCode = a.AffCode,
                    PhoneNo = a.PhoneNo,
                    PhoneVerified = a.PhoneVerified,
                    DepositStatus = a.ActStatus,
                    Status = a.Status,
                    RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero),
                    LastActiveTime = new DateTimeOffset(a.LastActiveTime, TimeSpan.Zero),
                    UpdateRemark = ZipRemark(a.UpdateAt, a.Operator, a.DepositStatus, timezone),
                    ReferralUrl = a.ReferralUrl,
                    SiteName = a.SiteName
                });
            return new LiveRegistrationResponse()
            {
                LiveRegistration = regLiveStatus.ToList(),
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount
            };
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetExportRegLiveStatus(ExportRegLiveStatusRequest request)
        {
            var custList = new List<int>();
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var timezone = request.StartDate.Offset;

            var (dormantFrequency, dormantType) = GetDormant(request.DormantUser);
			var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            if (request.DateType == LiveRegistrationDateTypeEnum.CallRecordDate 
                || !string.IsNullOrEmpty(request.UserName))
            {
                custList = await GetQueryCustId(request.DateType,request.SiteIds ,request.SiteId, startTime, endTime, request.UserName);
                if (custList == null || custList.Count == 0)
                    return new List<Dictionary<string, object>>();
            }
            var result = await _registrationService.GetExportRegLiveStatus(JsonConvert.SerializeObject(multiSiteRequest),
                request.DepositStatus, request.Status, custList, startTime, endTime,
                request.AffiliatesCode, request.AgentCodeType, dormantFrequency, dormantType, request.CallOutStatus,
                (int)request.DateType);

            int maxCol = 0;
            var liveRegs = new List<ExportLiveRegistration>();
            foreach (var item in result)
            {
                var liveReg = new ExportLiveRegistration()
                {
                    UserName = item.UserName,
                    FullName = item.AccountName,
                    AffCode = item.AffCode,
                    PhoneNo = item.PhoneNo,
                    RegTime = new DateTimeOffset(item.RegTime, TimeSpan.Zero).ToOffset(timezone),
                    LastActiveTime = new DateTimeOffset(item.LastActiveTime, TimeSpan.Zero).ToOffset(timezone),
                    DepositStatus = Enum.GetName(typeof(ActStatusEnum), item.ActStatus),
                    Note = item.Note,
                    CallRecord = ZipRemark(item.UpdateAt, item.Operator, item.DepositStatus, timezone),
                    ReferralUrl = item.ReferralUrl,
                    BonusCode = item.BonusCode,
                    PromotionCategory = item.PromotionCategory,
                    FTDDate = new DateTimeOffset(item.FTDDate, TimeSpan.Zero).ToOffset(timezone),
                    Amount = item.Amount,
					SiteName = item.SiteName
                };
                if (liveReg.CallRecord?.Count > maxCol)
                    maxCol = liveReg.CallRecord.Count;
                liveRegs.Add(liveReg);
            }

            var exportData = liveRegs.Select(a =>
            {
                var liveReg = new Dictionary<string, object>()
                {
                    { "UserName", a.UserName },
                    { "FullName", a.FullName },
                    { "SiteName", a.SiteName },
                    { "PhoneNo", a.PhoneNo },
                    { "RegTime", a.RegTime },
                    { "LastActiveTime", a.LastActiveTime },
                    { "DepositStatus", a.DepositStatus },
                    { "AffiliatesCode", a.AffCode },
                    { "Note", a.Note },
                    { "ReferralUrl", a.ReferralUrl },
                    { "FTD Date", a.FTDDate.Equals(default) ? "" : a.FTDDate.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "BonusCode", a.BonusCode },
                    { "Promotion Category", a.PromotionCategory },
                    { "Locked Amount/FTD Amount", a.Amount }
                };
                for (int i = 0; i < maxCol; i++)
                {
                    if (i < a.CallRecord?.Count)
                        liveReg.Add($"{i + 1} CallRecord", a.CallRecord[i]);
                    else
                        liveReg.Add($"{i + 1} CallRecord", "");
                }
                return liveReg;
            });

            return exportData;
        }

        public async Task<CustRegLiveStatus> GetRegLiveStatus(int custId)
        {
            return await _registrationService.GetRegLiveStatus(custId);
        }

        public async Task<int> SetRegLiveStatus(SetRegLiveStatusRequest input, string @operator)
        {
            return await _registrationService.SetRegLiveStatus(input.CustId, input.SiteId, (int)input.Status, @operator);
        }

        public async Task<int> AddCallingRecord(int siteId, int custId, string @operator)
        {
            return await _registrationService.AddCallingRecord(siteId, custId, @operator);
        }

        public async Task<IEnumerable<CustRegLiveStatusNotes>> GetRegLiveStatusNotes(int custId)
        {
            var data = await _registrationService.GetRegLiveStatusNotesByCustId(custId);
            return data.Select(a => new CustRegLiveStatusNotes()
            {
                Id = a.Id,
                CustId = a.CustId,
                Note = a.Note,
                Operator = a.Operator,
                UpdateTime = new DateTimeOffset(a.UpdateTime, TimeSpan.Zero)
            });
        }
        public async Task<CustRegLiveStatusNotesData> GetRegLiveStatusNotesById(int id)
        {
            return await _registrationService.GetRegLiveStatusNotesById(id);
        }
        public async Task<int> AddRegLiveStatusNote(int custId, string note, string @operator)
        {
            return await _registrationService.AddRegLiveStatusNote(custId, note, @operator);
        }

        public async Task<int> DeleteRegLiveStatusNote(int custId, int id, string @operator)
        {
            return await _registrationService.DeleteRegLiveStatusNote(custId, id);
        }

        public async Task<int> EditRegLiveStatusNote(int custId, int id, string note, string @operator)
        {
            return await _registrationService.EditRegLiveStatusNote(custId, id, note, @operator);
        }

        private List<string> ZipRemark(string rawUpdateTime, string rawUpdateOperator, string rawDepositStatus, TimeSpan timezone)
        {
            if (string.IsNullOrEmpty(rawUpdateTime) || string.IsNullOrEmpty(rawUpdateOperator)) return null;

            var updateTime = rawUpdateTime.Split(',');
            var updateOperator = rawUpdateOperator.Split(',');
            var depositStatus = rawDepositStatus.Split(',');
            return updateTime.Zip(updateOperator, (c, n) =>
            {
                var originTime = DateTime.Parse(c);
                var timeOffset = new DateTimeOffset(originTime, TimeSpan.Zero);
                var datetime = timeOffset.ToOffset(timezone).DateTime;
                return $"{datetime:yyyy-MM-dd HH:mm:ss}-{n}";
            }).Zip(depositStatus, (x, y) =>
            {
                var status = Convert.ToInt32(y);
                if (status == (int)CustomerTeleStatusEnum.NoRecord)
                    return x;
                else
                    return $"{x}({Enum.GetName(typeof(CustomerTeleStatusEnum), status)})";
            }).ToList();
        }

        private (int dormantFrequency, int dormantType) GetDormant(int dormantUser)
        {
            var frequency = dormantUser / 10;
            var type = dormantUser % 10;
            return (frequency, type);
        }

        private async Task<List<int>> GetQueryCustId(LiveRegistrationDateTypeEnum dateType, IEnumerable<int> siteIds,int siteId, DateTime startTime, DateTime endTime, string userName)
        {
            var custList = new List<int>();
            var selectCustList = new List<CustomerForRisk>();
            
            foreach (var siteIdTest in siteIds)
            {
                if (dateType == LiveRegistrationDateTypeEnum.CallRecordDate)
                {
                    var callRecord = await _registrationService.QueryCallingRecord(siteIdTest, startTime, endTime);
                    if (!callRecord.Any())
                    {
                        continue;
                    } 
                    custList.AddRange(callRecord);
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    var cust = await _memberService.GetCustId(null, userName, siteIdTest);
                    if (cust == null) 
                        continue;
                    
                    selectCustList.Add(cust);
                }
            }
            
            if (selectCustList.Count != 0)
            {
                custList = dateType == LiveRegistrationDateTypeEnum.CallRecordDate ? 
                    custList.Intersect(selectCustList.Select(c => c.CustId)).ToList() : 
                    selectCustList.Select(x => x.CustId).ToList();
            }
            
            return custList;
        }
    }
}