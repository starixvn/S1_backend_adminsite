using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class AdminUserWithAllowSite : AdminUser
    {
        public Dictionary<int, string> AllowSiteList { set; get; }
    }

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

    public class AdminUserV2
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Userpwd { set; get; }
        public bool IsSuper { set; get; }
        public int Status { set; get; }
        public DateTime CreateTime { set; get; }
        public string Role { get; set; }
        public string CurrencyList { get; set; }
        public int SiteId { get; set; }
        public DateTime? LoginTime { set; get; }
    }
    public class UpdataAdminUserV2Role
    {
        public int UserId { set; get; }
        public List<int> Role { get; set; }
    }

    public class AssignBrand
    {
        public int UserId { get; set; }
        public int SiteId { set; get; }
        public string CurrencyList { get; set; }
    }
}
