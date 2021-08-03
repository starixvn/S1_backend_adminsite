using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AdminUser
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public bool IsSuper { set; get; }
        public int Status { set; get; }
        public DateTime CreateTime { set; get; }
        public int? GroupSEQ { get; set; }
        public string GroupName { get; set; }
    }

    public class AuthData
    {
        public int UserId { set; get; }
        public string UserName { set; get; }

        //public string Token { set; get; }
        public string LangId { set; get; }
    }

    public class ChangePwd
    {
        public string OldPwd { set; get; }
        public string NewPwd { set; get; }

        /*public string Token { set; get; }
        public string LangId { set; get; }*/
    }

    public class AssignSites
    {
        public int UserId { get; set; }
        public int SiteId { set; get; }
    }

    public class AssignGroups
    {
        public int UserId { get; set; }
        public int GroupSEQ { get; set; }
    }
}
