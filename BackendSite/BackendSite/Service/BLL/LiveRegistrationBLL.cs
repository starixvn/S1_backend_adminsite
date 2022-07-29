using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationBLL
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

        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(int selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var searchResult = await _registrationService.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType);
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
                RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero)
            });
            return new LiveRegistrationResponse()
            {
                LiveRegistration = regLiveStatus.ToList(),
                PageNumber = searchResult.PageNumber,
                TotalPages = searchResult.TotalPages,
                RowCount = searchResult.RowCount
            };
        }

        public async Task<CustRegLiveStatus> GetRegLiveStatusByCustId(int custId)
        {
            return await _registrationService.GetRegLiveStatusByCustId(custId);
        }
        
        public async Task<int> SetRegLiveStatus(ParamsWithLog<SetRegLiveStatus> input)
        {
            var result= await _registrationService.SetRegLiveStatus(input.After.CustId, input.SiteId, (int)input.After.Status, input.Operator);
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = input.Operator,
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = input.Before.UserName,
                    Status = input.Before.StatusName
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = input.After.UserName,
                    Status = input.After.StatusName
                }),
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics
            };
            if (result <= 0) return result;
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }

            return result;
        }

        public async Task<int> UpdateCustomerTele(int siteId, int custId, string @operator)
        {
            return await _registrationService.UpdateCustomerTele(siteId, custId, @operator);
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

        public async Task<IEnumerable<CustRegLiveStatusNotesData>> GetRegLiveStatusNotesByCustId(int custId)
        {
            return await _registrationService.GetRegLiveStatusNotesByCustId(custId);
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
        public IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(string siteIds, int currency, int depositStatus, int status, string user, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var list = new List<RegLiveStatusExport>();
            var result = _registrationService.GetExportRegLiveStatus(siteIds, currency, depositStatus, status, user, sort, startDate, endDate, affiliatesCode, agentCodeType);
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
    }
}