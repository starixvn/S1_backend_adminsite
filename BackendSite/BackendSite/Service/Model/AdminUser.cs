using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AdminUser
    {
        public int UserId { set; get; }
        public int SiteId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public bool IsAdmin { set; get; }
        public int Status { set; get; }
    }
    public class AuthData
    {
        public int UserId { set; get; }
        public string UserName { set; get; }

        /*public string Token { set; get; }
        public string LangId { set; get; }*/
    }
}
