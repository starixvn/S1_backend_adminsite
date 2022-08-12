using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationBLL : ILiveRegistrationBLL
    {
        private readonly LiveRegistrationService _registrationService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly ILogger<LiveRegistrationBLL> _logger;
        
        public LiveRegistrationBLL(LiveRegistrationService registrationService, AdjustmentLogReportService adjustmentLogReportService, ILogger<LiveRegistrationBLL> logger)
        {
            _registrationService = registrationService;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
        }

        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(int selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantUser, int callOutStatus, int dateType)
        {
            var searchResult = await _registrationService.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType, dormantUser, callOutStatus, dateType);
            var regLiveStatus = searchResult.RegLiveStatusData.Select(a => new LiveRegistration()
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
                UpdateRemark = ZipRemark(a.UpdateAt, a.Operator)
            });
            return new LiveRegistrationResponse()
            {
                LiveRegistration = regLiveStatus.ToList(),
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount
            };
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
            return await _registrationService.EditRegLiveStatusNote(custId, id ,note, @operator);
        }
        public IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(int siteId, int currency, int depositStatus, int status, string user, int sort, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantUser, int callOutStatus, int dateType)
        {
            var list = new List<RegLiveStatusExport>();
            var result = _registrationService.GetExportRegLiveStatus(siteId, currency, depositStatus, status, user, sort, startDate, endDate, affiliatesCode, agentCodeType,dormantUser, callOutStatus, dateType);
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
                    Status = Enum.GetName(typeof(CustomerRegLiveStatusStatusEnum), x.Status),
                    AffCode = x.AffCode,
                    Agent = x.Agent
                });
            });
            return list;
        }

        private List<string> ZipRemark(string rawUpdateTime, string rawUpdateOperator)
        {
            if (string.IsNullOrEmpty(rawUpdateTime) || string.IsNullOrEmpty(rawUpdateOperator)) return null;

            var updateTime = rawUpdateTime.Split(',');
            var updateOperator = rawUpdateOperator.Split(',');
            return updateTime.Zip(updateOperator, (c, n) => $"{c}-{n}").ToList();
        }
    }
}