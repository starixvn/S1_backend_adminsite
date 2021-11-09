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

    public class DisableDisableUser
    {
        public int CustId { get; set; }
        public int Status { get; set; }
    }

    public class DisableTpGame
    {
        public int TpId { get; set; }
        public List<string> GameList { get; set; }
    }

    public class QueryTpDsiableUser
    {
        public IEnumerable<int> SiteIds { get; set; }
        public string UserName { get; set; }
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
        public int Status { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string UpdateOperator { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
    }

    public class TpDisableUserDetail
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public string GameInfo { get; set; }
    }

    public class TpDisableUserForGameDetail
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public List<RiskTpListInfo> DisableGames { get; set; }
    }
    public class SiteTpListIfo
    {
        public int TpId { set; get; }
        public string TpName { set; get; }
    }
    
    public class RiskTpListInfo
    {
        public int TpId { set; get; }
        public string TpName { set; get; }
        public List<TpGameCasino> GameList { set; get; }
        public bool Checked { set; get; }
    }
    
    public class TpGameCasino
    {
        public string GameId { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
    
    public class GameCasinoDataV3
    {
        public int GameCasinoId { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
    }

    public class RiskGameCasinoData
    {
        public int GameCasinoId { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public string CName { get; set; }
        public bool Checked { get; set; }
    }

    public class GameCasinoTranslate
    {
        public string en { get; set; }
        public string cs { get; set; }
    }
}