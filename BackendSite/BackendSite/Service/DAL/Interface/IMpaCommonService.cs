using BackendSite.Service.Model.Mpa.WinLoss;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaCommonService
    {
        /// <summary>
        /// 取得指定期間內的匯率資料
        /// </summary>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<IEnumerable<MpaRateDto>> GetMpaExchangeRate(DateTimeOffset starTime, DateTimeOffset endTime);


        /// <summary>
        /// 依據資料日期取得指定幣別當天需要的匯率
        /// </summary>
        /// <param name="source"></param>
        /// <param name="date"></param>
        /// <param name="currencyId"></param>
        /// <param name="baseCurrencyId"></param>
        /// <returns></returns>
        decimal MapperExchangeRate(IEnumerable<MpaRateDto> source, DateTime date, int currencyId, int baseCurrencyId);

        /// <summary>
        /// 取得幣別對應
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<int, string>> GetCurrencyIsoCodeMapper();

    }
}