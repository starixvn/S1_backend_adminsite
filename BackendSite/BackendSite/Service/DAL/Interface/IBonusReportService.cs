using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IBonusReportService
    {
        /// <summary>
        /// 取得BonusReport List 清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BonusReportDtoWrapper> GetBonusReportV2Async(BonusReportRequestV2 request);


        /// <summary>
        /// 取得BonusReport 匯出 Excel 資料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IEnumerable<BonusReportDto>> GetBonusReportExportData(BonusReportExportRequestV2 request);

        /// <summary>
        /// 取得PromotionCategory 下拉選單內容
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PromotionCategoryOptionDto>> GetPromotionCategoryOption();
    }
}
