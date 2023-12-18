using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class WinLostReportBLL
    {
        #region Fields

        private readonly WinLostReportService winLostReportService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;

        #endregion

        #region Constructors

        public WinLostReportBLL(WinLostReportService winLostReportService, ServerInfoBLL serverInfoBLL, SelLangBLL selLangBLL)
        {
            this.winLostReportService = winLostReportService;
            this.serverInfoBLL = serverInfoBLL;
            this.selLangBLL = selLangBLL;
        }

        #endregion

        #region Public Methods

        public IEnumerable<GameType> GetTpGameType(string siteIds,int currencyId)
        {
            return winLostReportService.GetAllGameType(siteIds,currencyId);
        }

        #endregion
    }
}
