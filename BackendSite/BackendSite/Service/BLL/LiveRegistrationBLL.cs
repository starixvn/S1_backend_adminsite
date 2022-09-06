using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationBLL : ILiveRegistrationBLL
    {
        private readonly LiveRegistrationService _registrationService;
        
        public LiveRegistrationBLL(LiveRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(GetRegLiveStatusRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var timezone = request.StartDate.Offset;
            
            var (dormantFrequency, dormantType) = GetDormant(request.DormantUser);

            var searchResult = await _registrationService.GetRegLiveStatusData(request.SiteId, request.CurrencyId,
                request.DepositStatus, request.Status, request.UserName, request.PageNumber, request.PageSize,
                request.Sort, startTime, endTime, request.AffiliatesCode, request.AgentCodeType, dormantFrequency,
                dormantType, request.CallOutStatus, request.DateType);
            
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
                    UpdateRemark = ZipRemark(a.UpdateAt, a.Operator, timezone)
                });
            return new LiveRegistrationResponse()
            {
                LiveRegistration = regLiveStatus.ToList(),
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount
            };
        }

        public async Task<IEnumerable<ExportLiveRegistration>> GetExportRegLiveStatus(ExportRegLiveStatusRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var timezone = request.StartDate.Offset;
            
            var (dormantFrequency, dormantType) = GetDormant(request.DormantUser);
            
            var result = await _registrationService.GetExportRegLiveStatus(request.SiteId, request.CurrencyId,
                request.DepositStatus, request.Status, request.UserName, request.Sort, startTime, endTime,
                request.AffiliatesCode, request.AgentCodeType, dormantFrequency, dormantType, request.CallOutStatus,
                request.DateType);
            
            var list = result.Select(x => 
                new ExportLiveRegistration() 
                {
                    UserName = x.UserName,
                    FullName = x.AccountName,
                    PhoneNo = x.PhoneNo,
                    RegTime = new DateTimeOffset(x.RegTime, TimeSpan.Zero),
                    LastActiveTime = new DateTimeOffset(x.LastActiveTime, TimeSpan.Zero),
                    DepositStatus = Enum.GetName(typeof(ActStatusEnum), x.ActStatus),
                    Note = x.Note,
                    CallRecord = ZipRemark(x.UpdateAt, x.Operator, timezone) 
                });
            return list;
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

        private List<string> ZipRemark(string rawUpdateTime, string rawUpdateOperator, TimeSpan timezone)
        {
            if (string.IsNullOrEmpty(rawUpdateTime) || string.IsNullOrEmpty(rawUpdateOperator)) return null;

            var updateTime = rawUpdateTime.Split(',');
            var updateOperator = rawUpdateOperator.Split(',');
            return updateTime.Zip(updateOperator, (c, n) =>
            {
                var originTime = DateTime.Parse(c);
                var timeOffset = new DateTimeOffset(originTime, TimeSpan.Zero);
                var datetime = timeOffset.ToOffset(timezone).DateTime;
                return $"{datetime:yyyy-MM-dd HH:mm:ss}-{n}";
            }).ToList();
        }

        private (int dormantFrequency, int dormantType) GetDormant(int dormantUser)
        {
            var frequency = dormantUser / 10;
            var type = dormantUser % 10;
            return (frequency, type);
        }
    }
}