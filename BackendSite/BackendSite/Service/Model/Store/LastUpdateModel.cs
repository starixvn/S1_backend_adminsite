using System;

namespace BackendSite.Service.Model.Store
{
    public class LastUpdateModel
    {
        public string LastUpdater { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}