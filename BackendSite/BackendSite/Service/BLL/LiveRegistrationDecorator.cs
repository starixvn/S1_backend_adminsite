using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationDecorator : ILiveRegistrationBLL
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILiveRegistrationBLL _liveRegistration;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly ILogger<LiveRegistrationDecorator> _logger;

        public LiveRegistrationDecorator( CustInfoBLL custInfoBll, ILiveRegistrationBLL liveRegistration, AdjustmentLogReportService adjustmentLogReportService, ILogger<LiveRegistrationDecorator> logger)
        {
            _custInfoBll = custInfoBll;
            _liveRegistration = liveRegistration;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
        }
        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(int siteId, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantUser, int callOutStatus, int dateType, TimeSpan timezone)
        {
            return await _liveRegistration.GetRegLiveStatusData(siteId, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType, dormantUser, callOutStatus, dateType, timezone);
        }

        public async Task<CustRegLiveStatus> GetRegLiveStatus(int custId)
        {
            return await _liveRegistration.GetRegLiveStatus(custId);
        }

        public async Task<int> SetRegLiveStatus(SetRegLiveStatusRequest input, string @operator)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(input.CustId);
            if (customer == null) return 0;

            var regLiveStatus = await _liveRegistration.GetRegLiveStatus(input.CustId);
            
            var result= await _liveRegistration.SetRegLiveStatus(input, @operator);
            if (result <= 0) return result;
            
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = @operator,
                SiteId = input.SiteId,
                CurrencyId = customer.CurrencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Status = regLiveStatus == null ? "Pending" : regLiveStatus.Status.ToString()
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Status = input.Status.ToString()
                }),
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }

            return result;
        }

        public async Task<int> AddCallingRecord(int siteId, int custId, string @operator)
        {
            return await _liveRegistration.AddCallingRecord(siteId, custId, @operator);
        }

        public async Task<IEnumerable<CustRegLiveStatusNotes>> GetRegLiveStatusNotes(int custId)
        {
            return await _liveRegistration.GetRegLiveStatusNotes(custId);
        }

        public Task<CustRegLiveStatusNotesData> GetRegLiveStatusNotesById(int id)
        {
            return _liveRegistration.GetRegLiveStatusNotesById(id);
        }

        public async Task<int> AddRegLiveStatusNote(int custId, string note, string @operator)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(custId);
            if (customer == null) return 0;
            
            var addResult = await _liveRegistration.AddRegLiveStatusNote(custId, note, @operator);
            if (addResult <= 0) return addResult;
            
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = @operator,
                SiteId = customer.SiteId,
                CurrencyId = customer.CurrencyId,
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Note = note
                }),
                ActionType = ActionType.Create,
                MenuEnum = AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }
            return addResult;
        }

        public async Task<int> DeleteRegLiveStatusNote(int custId, int id, string @operator)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(custId);
            if (customer == null) return 0;
            
            var note = await _liveRegistration.GetRegLiveStatusNotesById(id);
            if (note == null) return 0;

            var deleteResult = await _liveRegistration.DeleteRegLiveStatusNote(custId, id, @operator);
            if (deleteResult <= 0) return deleteResult;
            
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = @operator,
                SiteId = customer.SiteId,
                CurrencyId = customer.CurrencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Note = note.Note
                }),
                ActionType = ActionType.Delete,
                MenuEnum = AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }

            return deleteResult;
        }

        public async Task<int> EditRegLiveStatusNote(int custId, int id, string note, string @operator)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(custId);
            if (customer == null) return 0;
            
            var noteData = await _liveRegistration.GetRegLiveStatusNotesById(id);
            if (noteData == null) return 0;
            
            var editResult = await _liveRegistration.EditRegLiveStatusNote(custId, id ,note, @operator);
            if (editResult <= 0) return editResult;
            
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = @operator,
                SiteId = customer.SiteId,
                CurrencyId = customer.CurrencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Note = noteData.Note
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    UserName = customer.UserName,
                    Note = note
                }),
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }

            return editResult;
        }

        public IEnumerable<ExportLiveRegistration> GetExportRegLiveStatus(int siteId, int currency, int depositStatus, int status, string user, int sort, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantUser, int callOutStatus, int dateType, TimeSpan timezone)
        {
            return _liveRegistration.GetExportRegLiveStatus(siteId, currency, depositStatus, status, user, sort, startDate, endDate, affiliatesCode, agentCodeType, dormantUser, callOutStatus, dateType, timezone);
        }
    }
}