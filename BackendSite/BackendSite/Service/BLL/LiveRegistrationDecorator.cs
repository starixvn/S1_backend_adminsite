using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class LiveRegistrationDecorator
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly LiveRegistrationBLL _liveRegistration;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly ILogger<LiveRegistrationDecorator> _logger;

        public LiveRegistrationDecorator( CustInfoBLL custInfoBll, LiveRegistrationBLL liveRegistration, AdjustmentLogReportService adjustmentLogReportService, ILogger<LiveRegistrationDecorator> logger)
        {
            _custInfoBll = custInfoBll;
            _liveRegistration = liveRegistration;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
        }
        public async Task<LiveRegistrationResponse> GetRegLiveStatusData(int selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            return await _liveRegistration.GetRegLiveStatusData(selectSite, currency, depositStatus, status, username, pageNumber, pageSize, sort, startDate, endDate, affiliatesCode, agentCodeType);
        }
        public async Task<int> SetRegLiveStatus(ParamsWithLog<SetRegLiveStatus> input)
        {
            var result= await _liveRegistration.SetRegLiveStatus(input);
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
            return await _liveRegistration.UpdateCustomerTele(siteId, custId, @operator);
        }

        public async Task<IEnumerable<CustRegLiveStatusNotes>> GetRegLiveStatusNotes(int custId)
        {
            return await _liveRegistration.GetRegLiveStatusNotes(custId);
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

        public IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(string siteIds, int currency, int depositStatus, int status, string user, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            return _liveRegistration.GetExportRegLiveStatus(siteIds, currency, depositStatus, status, user, sort, startDate, endDate, affiliatesCode, agentCodeType);
        }
    }
}