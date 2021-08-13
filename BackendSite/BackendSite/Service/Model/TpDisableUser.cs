using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class TpDisableUser
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public List<TpGame> TpGame { get; set; }
        public string UpdateOperator { get; set; }
    }

    public class TpGame
    {
        public int TpId { get; set; }
        public string GameId { get; set; }
    }
}