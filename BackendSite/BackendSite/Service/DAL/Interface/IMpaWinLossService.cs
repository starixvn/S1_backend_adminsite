using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaWinLossService
    {
        /// <summary>
        /// 取得 TpBetRecord Relay By (日期(天) 或 月份)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <param name="format"></param>
        /// <param name="memberTypeJson"></param>
        /// <param name="mpaDateType"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetMapTpBetRecord<TResult>(IEnumerable<int> siteIds, DateTimeOffset starTime,
            DateTimeOffset endTime, string format, string memberTypeJson, string userNameJson, MpaDateType mpaDateType = MpaDateType.SettledDate);


        /// <summary>
        /// 取得 TpBetRecord Relay By (日期(天) 或 月份)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <param name="format"></param>
        /// <param name="mpaDateType"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetMapTpBetRecordByPlayer<TResult>(int siteId, DateTimeOffset starTime,
            DateTimeOffset endTime, string format, MpaDateType mpaDateType = MpaDateType.SettledDate);


        /// <summary>
        /// 取得 Deposit 與 WithDraw Relay By (日期(天) 或 月份)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <param name="format"></param>
        /// <param name="memberTypeJson"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetMapTransaction<TResult>(IEnumerable<int> siteIds, DateTimeOffset starTime,
            DateTimeOffset endTime, string format, string memberTypeJson, string userNameJson);


        /// <summary>
        /// 取得所有 TpGameTypeID/TpGameTypeName
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TpGameTypeOptionDto>> GetTpGameTypeOption();


        /// <summary>
        /// 取得 9 W 傭金
        /// </summary>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <param name="format"></param>
        /// <param name="mpaDateType"></param>
        /// <returns></returns>
        IEnumerable<Commission9WDto> Get9WCommission(DateTimeOffset starTime,
            DateTimeOffset endTime, string format, MpaDateType mpaDateType = MpaDateType.SettledDate);

        /// <summary>
        /// 取得 TpCategoryId + TpCategoryName
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TpCategoryDto>> GetTpCategory();

        /// <summary>
        /// 取得Bonus
        /// </summary>
        /// <param name="siteIds">站台Id</param>
        /// <param name="starTime">起始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <param name="format">日期 format 格式</param>
        /// <param name="memberTypeJson">玩家類型</param>
        /// <returns>Bonus 資訊</returns>
        /// <returns></returns>
        Task<IEnumerable<MpaWinLossBonusDto>> GetMpaWinLossBonus(IEnumerable<int> siteIds, DateTimeOffset starTime, DateTimeOffset endTime, string format, string memberTypeJson);
    }
}
