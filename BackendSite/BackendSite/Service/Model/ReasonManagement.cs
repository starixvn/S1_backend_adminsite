using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DateTime CreateTime { get; set; }
        public string Creator { get; set; }
        public DateTime UpdateTime { get; set; }
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

    }
}
