using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class EditTagManagement
    {
        [LogIgnore]
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagColour { get; set; }
        public string Description { get; set; }
    }
    public class SetTagStatus
    {
        [LogIgnore]
        public int Id { get; set; }
        [LogIgnore]
        public GeneralStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public string TagName { get; set; }
    }
    
    public class SetCustTag
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<int> SelectTag { get; set; }
    }
}