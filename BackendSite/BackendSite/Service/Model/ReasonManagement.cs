using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class ReasonManagement
    {
    }
    public class ReasonManagementFuncKey
    {
        public string FuncKey { get; set; }
    }
    public class GetReasonManagementDataPara
    {
        public string FuncKey { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class ReasonManagementData
    {
        public int Id { get; set; }
        public string FuncKey { get; set; }
        public int ReasonId { get; set; }
        public string Description { get; set; }
        public string CreateTime { get; set; }
        public string Creator { get; set; }
        public string UpdateTime { get; set; }
        public string Updator { get; set; }
    }

    public class GetReasonManagementDataResult
    {
        public List<ReasonManagementData> ReasonManagementData { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class SetReasonManagementPara
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }
    public class InsertReasonManagementPara
    {
        public string FuncKey { get; set; }
        public string Description { get; set; }
    }
}
