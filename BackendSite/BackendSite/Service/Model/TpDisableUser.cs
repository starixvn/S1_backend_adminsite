using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class DisableUser
    {
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public List<DisableTpGame> DisableTpGame { get; set; }
    }

    public class DisableTpGame
    {
        public int TpId { get; set; }
        public List<string> GameList { get; set; }
    }

    public class QueryTpDsiableUser
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class TpDisableUser
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }
        public string UpdateOperator { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}