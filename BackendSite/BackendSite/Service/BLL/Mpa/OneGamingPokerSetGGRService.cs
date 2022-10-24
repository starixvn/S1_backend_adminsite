using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.BLL.Mpa
{
    public class OneGamingPokerSetGGRService
    {
        /// <summary>
        /// For p2p products (OneGaming Poker), show commission: GGR = sum(revenue) 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public void SetGGR(MpaTpBetRecordByDateDto source)
        {
            source.WinLossSummary = source.RevenueSummary;
        }
    }
}
