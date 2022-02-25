using System;
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
        public IEnumerable<string> FuncKey { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class ReasonManagementData
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public int Id { get; set; }
        public string FuncKey { get; set; }
        public int ReasonId { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreateTime 
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Creator { get; set; }
        public DateTimeOffset UpdateTime 
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
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
