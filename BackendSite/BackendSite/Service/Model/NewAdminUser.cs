using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class NewAdminUser
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public bool IsSuper { set; get; }
        public int Status { set; get; }
        public DateTime CreateTime { get; set; }
        public string CurrencyList { get; set; }
        public int SiteId { get; set; }
    }
}
