using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.StorePoint
{
    public class ImportManualRewardPointInfoResult
    {
        public IEnumerable<ManualRewardPointInfo> Datas { get; set; }
        public IEnumerable<int> FailedRows { get; set; }
    }
    
    public class SelectListItem
    {
        public SelectListItem(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}