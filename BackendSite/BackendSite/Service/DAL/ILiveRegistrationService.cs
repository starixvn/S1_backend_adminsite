using BackendSite.Service.Model;

namespace BackendSite.Service.DAL
{
    public interface ILiveRegistrationService
    {
        Task<int> AddCallingRecord(int siteId, int custId, string @operator);
        Task<int> AddRegLiveStatusNote(int custId, string note, string @operator);
        Task<int> DeleteRegLiveStatusNote(int custId, int id);
        Task<int> EditRegLiveStatusNote(int custId, int id, string note, string @operator);
        Task<IEnumerable<ExportRegLiveStatusData>> GetExportRegLiveStatus(string multiSiteRequest, int depositStatus, int status, List<int> custIdList, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantFrequency, int dormantType, int callOutStatus, int dateType);
        Task<CustRegLiveStatus> GetRegLiveStatus(int custId);
        Task<RegLiveStatusPageResult> GetRegLiveStatusData(string multiSiteRequest, int depositStatus, int status, List<int> custIdList, int dateType, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantType, int dormantFrequency, int callOutStatus, string orderByProperty, int orderByDirection, int pageNumber, int pageSize);
        Task<IEnumerable<CustRegLiveStatusNotesData>> GetRegLiveStatusNotesByCustId(int custId);
        Task<CustRegLiveStatusNotesData> GetRegLiveStatusNotesById(int id);
        Task<IEnumerable<int>> QueryCallingRecord(int siteId, DateTime startTime, DateTime endTime);
        Task<int> SetRegLiveStatus(int custId, int siteId, int status, string @operator);
    }
}