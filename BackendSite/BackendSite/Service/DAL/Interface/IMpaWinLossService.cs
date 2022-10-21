using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaWinLossService
    {
        /// <summary>
        /// 取得 TpBetRecord Relay By (日期(天) 或 月份)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="settledStarTime"></param>
        /// <param name="settledEndTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetMapTpBetRecordByDate<TResult>(int siteId, DateTime settledStarTime,
            DateTime settledEndTime, string format);

        /// <summary>
        /// 取得 Deposit 與 WithDraw Relay By (日期(天) 或 月份)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="settledStarTime"></param>
        /// <param name="settledEndTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetMapTransactionByDate<TResult>(int siteId, DateTime settledStarTime,
            DateTime settledEndTime, string format);

        /// <summary>
        /// 取的幣別的 USD 匯率，目前先從 Currency 取的，未來可能替換成 匯率商 API
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MpaUSdRateDto>> GetMpaUsdRate();

        /// <summary>
        /// 透過 UserName/tpUserName 取得 CustId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<int> GetCustIdByUserName(int siteId, string userName);


        /// <summary>
        /// 取得所有 TpGameTypeID/TpGameTypeName
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TpGameTypeOptionDto>> GetTpGameTypeOption();
    }
}
