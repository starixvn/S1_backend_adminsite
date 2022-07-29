using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface ILiveRegistrationBLL
    {
        Task<LiveRegistrationResponse> GetRegLiveStatusData(int selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType);
        Task<CustRegLiveStatus> GetRegLiveStatus(int custId);
        Task<int> SetRegLiveStatus(SetRegLiveStatusRequest input, string @operator);
        Task<int> AddCallingRecord(int siteId, int custId, string @operator);
        Task<IEnumerable<CustRegLiveStatusNotes>> GetRegLiveStatusNotes(int custId);
        Task<CustRegLiveStatusNotesData> GetRegLiveStatusNotesById(int id);
        Task<int> AddRegLiveStatusNote(int custId, string note, string @operator);
        Task<int> DeleteRegLiveStatusNote(int custId, int id, string @operator);
        Task<int> EditRegLiveStatusNote(int custId, int id, string note, string @operator);
        IEnumerable<RegLiveStatusExport> GetExportRegLiveStatus(string siteIds, int currency, int depositStatus, int status, string user, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType);
    }
}