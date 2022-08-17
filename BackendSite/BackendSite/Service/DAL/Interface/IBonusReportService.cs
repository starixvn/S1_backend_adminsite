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
        Task<BonusReportDtoWrapper> GetBonusReportAsync(BonusReportRequest request);


        /// <summary>
        /// 取得BonusReport 匯出 Excel 資料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IEnumerable<BonusReportDto>> GetBonusReportExportData(BonusReportExportRequest request);
    }
}
