using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.StorePoint
{
    public class ImportManualRewardPointInfoResult
    {
        public IEnumerable<ManualRewardPointInfo> Datas { get; set; }
        public IEnumerable<int> FailedRows { get; set; }
    }
}