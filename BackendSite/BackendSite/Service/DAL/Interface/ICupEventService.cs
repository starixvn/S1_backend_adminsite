using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface ICupEventService
    {
        Task BeginTransaction();
        Task Commit();
        Task RollBack();

        /// <summary>
        /// 取得後台使用者設定的世界盃設定
        /// </summary>
        /// <param name="langId">語系Id，看前端傳什語系</param>
        /// <returns></returns>
        IEnumerable<CupEventSettingDto> GetCupEventSetting(string langId);

        /// <summary>
        /// 取得世界盃國家下拉選單內容
        /// </summary>
        /// <returns></returns>
        IEnumerable<CupEventOptionsDto> GetCupEventOptions(string langId);

        /// <summary>
        /// 更新 Status 狀態
        /// </summary>
        /// <returns></returns>
        Task<int> SetCupEventPrediction();

        /// <summary>
        /// 寫入 CupEventSetting
        /// </summary>
        /// <param name="setData"></param>
        /// <returns></returns>
        Task<int> InsertCupEventPrediction(CupEventSettingDto setData);

        IEnumerable<CupEventMemberReportDto> GetMemberReport(GetCupEventMemberReportRequest request, out int totalPages, out int rowCount);
        Task<IEnumerable<CupEventMemberReportDto>> GetMemberReport(ExportCupEventMemberReportRequest request);
        Task<IEnumerable<CupEventPredictiveStatisticsDto>> GetPredictiveStatistics(int siteId);
        Task<IEnumerable<CupEventPredictDetailDto>> GetPredictDetail(int siteId, int teamId);
        Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator);
        Task<IEnumerable<CupEventPredictScore>> GetCupEventPredictScore();
    }
}