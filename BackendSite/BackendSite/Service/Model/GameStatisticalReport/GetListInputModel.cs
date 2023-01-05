using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model.GameStatisticalReport
{
    public class GetListInputModel
    {
        public int SiteId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
