using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class DisableUser
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public List<DisableTpGame> DisableTpGame { get; set; }
        public string UpdateOperator { get; set; }
    }

    public class DisableTpGame
    {
        public int TpId { get; set; }
        public List<string> GameList { get; set; }
    }
}