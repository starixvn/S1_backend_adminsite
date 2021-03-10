using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class GameCasinoManage
    {
    }
    public class GameCasinoResult
    {
        public List<GameCasinoData> GameCasinoData { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class GameCasinoData
    {
        public int GameCasinoId { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string AppId { get; set; }
        public string Name { get; set; }
        public string Ename { get; set; }
        public string Cname { get; set; }
        public int Site { get; set; }
        public int Currency { get; set; }
        public string HasDetail { get; set;  }
        public string TpName { get; set; }

        //NEW
        public bool Hot { get; set; }
        public bool Status { get; set; }
        public bool Featured { get; set; }
        public bool Promotion { get; set; }
    }

    public class GameCasinoPara
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Site { get; set; }
        public int Currency { get; set; }
        public int TPid { get; set; }
        public string Name { get; set; }
        public GameCasinoPara()
        {
            //PageSize預設10
            if (PageSize == 0)
                PageSize = 10;
        }
    }

    public class GameCasinoDetail 
    { 

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GameCasinoId { get; set; }
        public int Hot { get; set; }
        public int Featured { get; set; }
        public int Status { get; set; }
        public int Promotion { get; set; }
        public DateTime UpdateAt { get; set; }
    }
    public class SetGameCasionPara
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GameCasinoId { get; set; }
        public int Hot { get; set; }
        public int Featured { get; set; }
        public int Status { get; set; }
        public int Promotion { get; set; }
        public string Type { get; set; }
    }
    public class InsertGameCasionPara
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GameCasinoId { get; set; }
    }
    public class DeleteGameCasionPara
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GameCasinoId { get; set; }
        public int Hot { get; set; }
        public int Featured { get; set; }
        public int Status { get; set; }
    }

    public class SetGameCasionV2Para
    {
        public int GameCasinoId { get; set; }
        public string Type { get; set; }
        public bool Hot { get; set; }
        public bool Featured { get; set; }
        public bool Status { get; set; }
        public bool Promotion { get; set; }
    }
    public class GetGameCasinoDetailPara
    {
        public int GameCasinoId { get; set; }
        public int SiteId { get; set; }
    }
    public class CurrencyDataV2
    {
        public int CurrencyId { get; set; }
        public int Tpid { get; set; }
        public string IsoCode { get; set; }
    }
    public class GameCasinoDetailData 
    {
        public bool HasDetail { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int GameCasinoId { get; set; }
        public int Hot { get; set; }
        public int Featured { get; set; }
        public int Status { get; set; }
        public int Promotion { get; set; }
    }
    public class GameCasinoDetailLog
    {
        public int GameCasinoId { get; set; }
        public int CurrencyId { get; set; }
        public string ChangeData { get; set; }
        public int Before { get; set; }
        public int After { get; set; }
        public int SiteId { get; set; }
    }
    public class GameCasinoLog
    {
        public int GameCasinoId { get; set; }
        public string ChangeData { get; set; }
        public int Before { get; set; }
        public int After { get; set; }
    }
}
