using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class LiveRegistrationService : ILiveRegistrationService
    {
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _mainDbClient;
        private readonly List<string> _creditSite = new() { "Nova88Ref01", "Nova88Ref02", "Nova88Ref03", "Nova88Ref04", "Nova88Ref05", "Nova88Ref06", "Nova88Ref07", "Nova88Ref08", "Goal123Ref01", "BwingRef01" };
        public LiveRegistrationService(IEnumerable<MySqlClient> mySqlClients)
        {
            _reportDbClient = mySqlClients.SingleOrDefault(a => a.DataBase == DataBase.Report);
            _mainDbClient = mySqlClients.SingleOrDefault(a => a.DataBase == DataBase.Main);
        }

        public async Task<RegLiveStatusPageResult> GetRegLiveStatusData(string multiSiteRequest, int depositStatus,
            int status, List<int> custIdList, int dateType, DateTime startDate, DateTime endDate,
            string affiliatesCode, int agentCodeType, int dormantType, int dormantFrequency, int callOutStatus,
            string orderByProperty, int orderByDirection, int pageNumber, int pageSize)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_CustIdList", JsonConvert.SerializeObject(custIdList));
            props.Add("_DepositStatus", depositStatus);
            props.Add("_Status", status);
            props.Add("_DateType", dateType);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_AffCode", affiliatesCode);
            props.Add("_CreditSite", JsonConvert.SerializeObject(_creditSite));
            props.Add("_AgentCodeType", agentCodeType);
            props.Add("_DormantType", dormantType);
            props.Add("_DormantFrequency", dormantFrequency);
            props.Add("_CallOutStatus", callOutStatus);
            props.Add("_OrderByProperty", orderByProperty);
            props.Add("_OrderByDirection", orderByDirection);
            props.Add("_PageSize", Math.Abs(pageSize));
            props.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("_PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _reportDbClient.QueryAsync<RegLiveStatusData>("Admin_CustRegLiveStatus_Get_V9", props);
            var searchData = new RegLiveStatusPageResult
            {
                RegLiveStatusData = result.ToList(),
                PageNumber = props.Get<int?>("_PageNumber") ?? 0,
                TotalPages = props.Get<int?>("_TotalPages") ?? 0,
                RowCount = props.Get<int?>("_RowCount") ?? 0
            };

            return searchData;
        }
        public async Task<IEnumerable<ExportRegLiveStatusData>> GetExportRegLiveStatus(string multiSiteRequest, int depositStatus, int status, List<int> custIdList, DateTime startDate, DateTime endDate, string affiliatesCode, int agentCodeType, int dormantFrequency, int dormantType, int callOutStatus, int dateType)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_CustIdList", JsonConvert.SerializeObject(custIdList));
            props.Add("_DepositStatus", depositStatus);
            props.Add("_Status", status);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_AffCode", affiliatesCode);
            props.Add("CreditSite", JsonConvert.SerializeObject(_creditSite));
            props.Add("_AgentCodeType", agentCodeType);
            props.Add("_DormantFrequency", dormantFrequency);
            props.Add("_DormantType", dormantType);
            props.Add("_CallOutStatus", callOutStatus);
            props.Add("_DateType", dateType);

            var result = await _reportDbClient.QueryAsync<ExportRegLiveStatusData>("Admin_CustRegLiveStatusExport_Get_V9", props);

            return result;
        }
        public async Task<CustRegLiveStatus> GetRegLiveStatus(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _reportDbClient.QueryFirstAsync<CustRegLiveStatus>("Admin_RegLiveStatus_Get", p);
        }
        public async Task<int> SetRegLiveStatus(int custId, int siteId, int status, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("SiteId", siteId);
            props.Add("Status", status);
            props.Add("Operator", @operator);
            return await _mainDbClient.ExecuteAsync("Admin_RegLiveStatus_Set", props);
        }
        public async Task<int> AddCallingRecord(int siteId, int custId, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", siteId);
            props.Add("CustId", custId);
            props.Add("Operator", @operator);
            return await _mainDbClient.ExecuteAsync("Admin_CustTele_Ins_V2", props);
        }
        public Task<IEnumerable<int>> QueryCallingRecord(int siteId, DateTime startTime, DateTime endTime)
            => _reportDbClient.QueryAsync<int>("Admin_CustTele_Get",
                new { _SiteId = siteId, _StartTime = startTime, _EndTime = endTime });

        public async Task<IEnumerable<CustRegLiveStatusNotesData>> GetRegLiveStatusNotesByCustId(int custId)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            return await _reportDbClient.QueryAsync<CustRegLiveStatusNotesData>("Admin_CustRegLiveStatusNotesByCustId_Get", props);
        }
        public async Task<CustRegLiveStatusNotesData> GetRegLiveStatusNotesById(int id)
        {
            var props = new DynamicParameters();
            props.Add("_Id", id);
            return await _reportDbClient.QueryFirstAsync<CustRegLiveStatusNotesData>("Admin_CustRegLiveStatusNotesById_Get", props);
        }
        public async Task<int> AddRegLiveStatusNote(int custId, string note, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("Note", string.IsNullOrWhiteSpace(note) ? "" : note);
            props.Add("Operator", @operator);
            return await _mainDbClient.ExecuteAsync("Admin_CustRegLiveStatusNotes_Ins", props);
        }
        public async Task<int> DeleteRegLiveStatusNote(int custId, int id)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("Id", id);
            return await _mainDbClient.ExecuteAsync("Admin_CustRegLiveStatusNotes_Del", props);
        }
        public async Task<int> EditRegLiveStatusNote(int custId, int id, string note, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("Id", id);
            props.Add("Note", note);
            props.Add("Operator", @operator);
            return await _mainDbClient.ExecuteAsync("Admin_CustRegLiveStatusNotes_Set", props);
        }
    }
}