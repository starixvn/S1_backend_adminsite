using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.BLL.Mpa
{
    public class V8PokerSetGGRService
    {
        /// <summary>
        /// For p2p products (V8 Poker), show commission: GGR = sum(revenue) 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public void SetGGR(MpaTpBetRecordByDateDto source)
        {
            source.WinLossSummary = source.RevenueSummary;
        }
    }
}
