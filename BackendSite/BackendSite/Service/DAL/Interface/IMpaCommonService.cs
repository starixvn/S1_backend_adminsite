using BackendSite.Service.Model.ApiModel;
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
        /// <param name="baseCurrencyId"></param>
        /// <returns></returns>
        Task<IEnumerable<MpaRateDto>> GetMpaExchangeRate(DateTimeOffset starTime, DateTimeOffset endTime, int baseCurrencyId = 0);


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


        /// <summary>
        /// 取得差異 % 數
        /// </summary>
        /// <param name="divisor"></param>
        /// <param name="dividend"></param>
        /// <returns></returns>
        string GetPercent(decimal divisor, decimal dividend);

        /// <summary>
        /// 處理註冊日期格是
        /// </summary>
        /// <param name="registeredDate"></param>
        /// <returns></returns>
        string RegisteredDateFormatter(DateTime registeredDate);


        /// <summary>
        /// Mpa 支援 Dictionary 排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="exportData"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Dictionary<TKey, TValue> SortExportData<TKey, TValue>(
            Dictionary<TKey, TValue> exportData,
            string sortField,
            OrderByDirection sortOrder);
    }
}